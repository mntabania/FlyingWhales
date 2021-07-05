using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using EZObjectPools;
using Inner_Maps;
using Locations.Settlements;
using Ruinarch.Custom_UI;

public class FactionInfoUIV2 : MonoBehaviour {

    [Space(10)]
    [Header("Overview")]
    [SerializeField] private CharacterNameplateItem leaderNameplateItem;
    [SerializeField] private GameObject noLeaderTextGO;
    [SerializeField] private TextMeshProUGUI ideologyLbl;
    [SerializeField] private CharacterPortrait[] successorPortraits;

    [Space(10)]
    [Header("Locations")]
    [SerializeField] private Transform locationsTransform;
    [SerializeField] private GameObject settlementNameplatePrefab;
    private List<SettlementNameplateItem> locationItems;

    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private ScrollRect relationshipsScrollRect;
    [SerializeField] private GameObject relationshipPrefab;

    [Space(10)]
    [Header("Crimes")]
    [SerializeField] private TextMeshProUGUI infractionCrimesLbl;
    [SerializeField] private TextMeshProUGUI misdemeanourCrimesLbl;
    [SerializeField] private TextMeshProUGUI seriousCrimesLbl;
    [SerializeField] private TextMeshProUGUI heinousCrimesLbl;
    [SerializeField] private ScrollRect crimesScrollRect;
    [SerializeField] private RectTransform crimesScrollRectTransform;

    //[SerializeField] private RectTransform[] crimesTransform;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private LogsWindow logsWindow;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private RuinarchToggle aliveToggle;
    [SerializeField] private GameObject traitFilterItemPrefab;
    [SerializeField] private GameObject regionFilterItemPrefab;
    [SerializeField] private ScrollRect traitFilterScrollRect;
    [SerializeField] private ScrollRect regionFilterScrollRect;
    [SerializeField] private TMP_InputField searchTraitFilterField;
    [SerializeField] private TMP_InputField searchRegionFilterField;

    private List<CharacterNameplateItem> _characterItems;
    private List<FactionTraitFilterItem> _traitFilterItems;
    private List<FactionRegionFilterItem> _regionFilterItems;

    private List<string> filteredTraits; //Characters must NOT have the traits inside this list
    private List<Region> filteredRegions;
    private List<BaseSettlement> filteredVillages;
    //private int traitFilterHalfCount;

    public GameObject overviewScrollRectParent;
    public GameObject membersScrollRectParent;
    public GameObject logScrollRectParent;
    public GameObject crimesScrollRectParent;
    public GameObject relationshipScrollRectParent;

    public GameObject btnRevealInfo;

    public Faction activeFaction { get; private set; }

    private enum VIEW_MODE { Overview = 0, Members, Crimes, Relations, Logs }
    private VIEW_MODE m_viewMode = VIEW_MODE.Members;

    public void Initialize() {
        _characterItems = new List<CharacterNameplateItem>();
        locationItems = new List<SettlementNameplateItem>();
        _traitFilterItems = new List<FactionTraitFilterItem>();
        _regionFilterItems = new List<FactionRegionFilterItem>();
        filteredTraits = new List<string>();
        filteredRegions = new List<Region>();
        filteredVillages = new List<BaseSettlement>();
        searchTraitFilterField.onValueChanged.AddListener(OnSearchTraitFilterValueChanged);
        searchRegionFilterField.onValueChanged.AddListener(OnSearchRegionFilterValueChanged);

        PopulateFilterTraits();

        ClearFilteredTraits();
        ClearFilteredRegions();

        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_ADDED, OnFactionSettlementAdded);
        Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_REMOVED, OnFactionSettlementRemoved);
        Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionRelationshipChanged);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
        Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnFactionLeaderChanged);
        Messenger.AddListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_IDEOLOGIES_CHANGED, OnFactionIdeologiesChanged);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_CRIMES_CHANGED, OnFactionCrimesChanged);
        //Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchFromLimbo);
        Messenger.AddListener<Character, Character>(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
        Messenger.AddListener<Faction>(FactionSignals.UPDATED_SUCCESSORS, OnFactionUpdatedSuccessors);
        logsWindow.Initialize();
        SetButtonRevealPriceDisplay();
    }

    void SetButtonRevealPriceDisplay() {
        btnRevealInfo.transform.Find("icon").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealFactionInfoCost().ToString();
    }

    public void SetFaction(Faction faction) {
        activeFaction = faction;
        if(activeFaction != null) {
            UpdateOverview();
            UpdateOwnedLocations();
            UpdateCrimes();
            UpdateAllRelationships();
            //UpdateFactionInfo();
            logsWindow.OnParentMenuOpened(activeFaction.persistentID);
            UpdateAllHistoryInfo();
            UpdateAllCharacters();
            ResetScrollPositions();
            ProcessDisplay();
            PopulateFilterRegions();
            InitializeRevealHoverText();
        }
    }
    //public void UpdateFactionInfo() {
    //    if (activeFaction == null) {
    //        return;
    //    }
    //    UpdateBasicInfo();
    //    //ResetScrollPositions();
    //}

    #region Overview
    private void OnCharacterDied(Character character) {
        if (activeFaction != null && character.faction == activeFaction) {
            FilterCharacters();
        }
    }
    private void OnFactionLeaderChanged(Character character, ILeader previousLeader) {
        if (activeFaction != null && character.faction == activeFaction) {
            UpdateOverview();
            UpdateAllCharacters();
        }
    }
    private void OnFactionIdeologiesChanged(Faction faction) {
        if (FactionInfoHubUI.Instance.IsShowing(faction)) {
            UpdateOverview();
        }
    }
    private void OnFactionCrimesChanged(Faction faction) {
        if (FactionInfoHubUI.Instance.IsShowing(faction)) {
            UpdateCrimes();
        }
    }
    private void OnFactionLeaderRemoved(Faction faction, ILeader previousLeader) {
        if (FactionInfoHubUI.Instance.IsShowing(faction)) {
            UpdateOverview();
        }
    }
    private void OnFactionUpdatedSuccessors(Faction faction) {
        if (activeFaction != null) {
            UpdateOverview();
        }
    }
    private void UpdateOverview() {
       
        if (activeFaction.leader is Character leader) {
            Character characterToShow = leader;
            if (leader.isLycanthrope) {
                characterToShow = leader.lycanData.activeForm;
            }
            leaderNameplateItem.gameObject.SetActive(true);
            leaderNameplateItem.SetObject(characterToShow);
            noLeaderTextGO.SetActive(false);
        } else {
            leaderNameplateItem.gameObject.SetActive(false);
            noLeaderTextGO.SetActive(true);
        }
        Character[] successors = activeFaction.successionComponent.successors;
        for (int i = 0; i < successorPortraits.Length; i++) {
            CharacterPortrait portrait = successorPortraits[i];
            Character successor = null;
            if (i >= 0 && i < successors.Length) {
                successor = successors[i];
            }
            if (successor == null) {
                portrait.gameObject.SetActive(false);
            } else {
                portrait.GeneratePortrait(successor);
                portrait.gameObject.SetActive(true);
            }
        }

        ideologyLbl.text = string.Empty;
        for (int i = 0; i < activeFaction.factionType.ideologies.Count; i++) {
            FactionIdeology ideology = activeFaction.factionType.ideologies[i];
            ideologyLbl.text += $"<sprite=\"Text_Sprites\" name=\"Arrow_Icon\">   <link=\"{i}\">{ideology.GetIdeologyName()}</link>\n";
        }    
    }
    public void OnHoverIdeology(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            FactionIdeology ideology = activeFaction.factionType.ideologies[index];
            UIManager.Instance.ShowSmallInfo(ideology.GetIdeologyDescription());
        }
    }
    public void OnHoverOutIdeology() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Locations
    private void UpdateOwnedLocations() {
        UtilityScripts.Utilities.DestroyChildren(locationsTransform);
        locationItems.Clear();
        for (int i = 0; i < activeFaction.ownedSettlements.Count; i++) {
            BaseSettlement ownedSettlement = activeFaction.ownedSettlements[i];
            CreateNewSettlementItem(ownedSettlement);
        }
    }
    private void CreateNewSettlementItem(BaseSettlement settlement) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(settlementNameplatePrefab.name, locationsTransform);
        SettlementNameplateItem item = characterGO.GetComponent<SettlementNameplateItem>();
        item.SetObject(settlement);
        item.SetAsButton();
        item.AddOnClickAction(OnClickSettlementItem);
        locationItems.Add(item);
    }
    private void OnClickSettlementItem(BaseSettlement settlement) {
        //if (settlement.allStructures.Count > 0) {
        //    UIManager.Instance.ShowStructureInfo(settlement.allStructures.First());
        //}
        UIManager.Instance.ShowSettlementInfo(settlement);
        // if (settlement.tiles.Count > 0) {
        //     HexTile tile = settlement.tiles[0];
        //     if (InnerMapManager.Instance.isAnInnerMapShowing) {
        //         //if inner map is showing, open inner map of hextile then center on it
        //         if (InnerMapManager.Instance.currentlyShowingLocation != tile.region) {
        //             InnerMapManager.Instance.TryShowLocationMap(tile.region);
        //         }
        //         InnerMapCameraMove.Instance.CenterCameraOnTile(tile);
        //     } else {
        //         //if world map is showing, just center on hextile
        //         tile.CenterCameraHere();
        //     }
        //     UIManager.Instance.ShowHexTileInfo(tile);
        // }
    }
    private SettlementNameplateItem GetLocationItem(BaseSettlement settlement) {
        for (int i = 0; i < locationItems.Count; i++) {
            SettlementNameplateItem locationPortrait = locationItems[i];
            if (locationPortrait.obj.id == settlement.id) {
                return locationPortrait;
            }
        }
        return null;
    }
    private void DestroyLocationItem(BaseSettlement settlement) {
        SettlementNameplateItem item = GetLocationItem(settlement);
        if (item != null) {
            locationItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }
    private void OnFactionSettlementAdded(Faction faction, BaseSettlement settlement) {
        if (FactionInfoHubUI.Instance.IsShowing(faction)) {
            CreateNewSettlementItem(settlement);
        }
    }
    private void OnFactionSettlementRemoved(Faction faction, BaseSettlement settlement) {
        if (FactionInfoHubUI.Instance.IsShowing(faction)) {
            DestroyLocationItem(settlement);
        }
    }
    #endregion

    #region Relationships
    public void UpdateAllRelationships() {
        UtilityScripts.Utilities.DestroyChildren(relationshipsScrollRect.content);

        foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in activeFaction.relationships) {
            if (keyValuePair.Key.factionType.type != FACTION_TYPE.Wild_Monsters && keyValuePair.Key.factionType.type != FACTION_TYPE.Disguised) {
                // if (keyValuePair.Key?.factionType.type == FACTION_TYPE.Undead && keyValuePair.Key.leader == null) {
                //     //Only add Undead faction in Relations once it gains a Faction Leader
                //     continue;
                // }
                GameObject relGO = UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsScrollRect.content);
                FactionRelationshipItem item = relGO.GetComponent<FactionRelationshipItem>();
                item.SetData(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
    private void OnFactionRelationshipChanged(Faction faction1, Faction faction2, FACTION_RELATIONSHIP_STATUS newStatus, FACTION_RELATIONSHIP_STATUS oldStatus) {
        if (FactionInfoHubUI.Instance.IsShowing(faction1) || FactionInfoHubUI.Instance.IsShowing(faction2)) {
            UpdateAllRelationships();
        }
    }
    private void OnFactionActiveChanged(Faction faction) {
        if (activeFaction != null) { //isShowing
            UpdateAllRelationships();
        }
    }
    #endregion

    #region Crimes
    private void UpdateCrimes() {
        crimesScrollRect.gameObject.SetActive(false);
        infractionCrimesLbl.text = string.Empty;
        misdemeanourCrimesLbl.text = string.Empty;
        seriousCrimesLbl.text = string.Empty;
        heinousCrimesLbl.text = string.Empty;

        TextMeshProUGUI crimeLbl = null;
        foreach (KeyValuePair<CRIME_TYPE, CRIME_SEVERITY> item in activeFaction.factionType.crimes) {
            string crimeType = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(item.Key.ToString());
            if (item.Value == CRIME_SEVERITY.Infraction) {
                crimeLbl = infractionCrimesLbl;
            } else if (item.Value == CRIME_SEVERITY.Misdemeanor) {
                crimeLbl = misdemeanourCrimesLbl;
            } else if (item.Value == CRIME_SEVERITY.Serious) {
                crimeLbl = seriousCrimesLbl;
            } else if (item.Value == CRIME_SEVERITY.Heinous) {
                crimeLbl = heinousCrimesLbl;
            }
            if(crimeLbl != null) {
                crimeLbl.text += $"<sprite=\"Text_Sprites\" name=\"Arrow_Icon\">   {crimeType}\n";
            }
        }
        
        //crimesScrollRectTransform.ForceUpdateRectTransforms();
        //Canvas.ForceUpdateCanvases();
        //for (int i = 0; i < crimesTransform.Length; i++) {
        //    crimesTransform[i].ForceUpdateRectTransforms();
        //}
    }
    #endregion

    #region Characters
    private void OnCharacterSetAsSettlementRuler(Character character, Character previousRuler) {
        if(activeFaction != null) {
            if ((character != null && character.faction == activeFaction) || (previousRuler != null && previousRuler.faction == activeFaction)) {
                UpdateAllCharacters();
            }
        }

    }
    private void OnCharacterSwitchFromLimbo(Character toLimbo, Character fromLimbo) {
        if(toLimbo.isLycanthrope) {
            Faction factionToBeFollowed = toLimbo.lycanData.originalForm.faction;
            if(activeFaction == factionToBeFollowed) {
                CharacterNameplateItem nameplate = GetItem(toLimbo);
                if(nameplate != null) {
                    nameplate.UpdateObject(fromLimbo);
                }
                if(leaderNameplateItem.character == toLimbo) {
                    leaderNameplateItem.UpdateObject(fromLimbo);
                }
            }
        } else {
            //TODO: Which faction should be followed the one from the limbo or the one going to limbo?
            //If both forms are from diff factions, the nameplate of each one will be shown at each faction UI, this will cause problems because only one must exist in the world at the same time
        }
    }
    private void UpdateAllCharacters() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        _characterItems.Clear();

        bool hasAliveMember = false;
        bool hasMember = false;
        //Angels should not show in the characters list of faction in UI
        //https://trello.com/c/SGow0hA0/2234-angels-on-list
        for (int i = 0; i < activeFaction.characters.Count; i++) {
            Character currCharacter = activeFaction.characters[i];
            if (currCharacter is Summon summon && summon.isVolatileMonster) {
                //skip volatile monster
                //this is a solution for https://trello.com/c/EbPTZFAv/2288-03314-skeleton-from-defiler-appears-on-vagrant-tab
                continue;
            }
            if (currCharacter.race != RACE.ANGEL) {
                if (currCharacter.isLycanthrope) {
                    currCharacter = currCharacter.lycanData.activeForm;
                }
                if (!currCharacter.isDead) {
                    hasAliveMember = true;
                }
                hasMember = true;
                CreateNewCharacterItem(currCharacter, false);
            }
        }
        if (!hasAliveMember && hasMember) {
            //If all faction members is dead, should auto toggle off the hide dead toggle, meaning all faction members even the dead ones, will be shown
            aliveToggle.isOn = false;
        }
        //OrderCharacterItems();
    }
    public void FilterCharacters() {
        for (int i = 0; i < _characterItems.Count; i++) {
            CharacterNameplateItem nameplate = _characterItems[i];
            if(nameplate.character != null) {
                nameplate.gameObject.SetActive(ShouldCharacterNameplateBeShown(nameplate.character));
            } else {
                nameplate.gameObject.SetActive(false);
            }
        }
    }
    private CharacterNameplateItem GetItem(Character character) {
        CharacterNameplateItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character != null) {
                if (item.character.id == character.id) {
                    return item;
                }
            }
        }
        return null;
    }
    private CharacterNameplateItem CreateNewCharacterItem(Character character, bool autoSort = true) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
        CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
        item.SetObject(character);
        item.SetAsDefaultBehaviour();
        characterGO.SetActive(ShouldCharacterNameplateBeShown(character));
        _characterItems.Add(item);
        //if (autoSort) {
        //    OrderCharacterItems();
        //}
        return item;
    }
    private CharacterNameplateItem GetCharacterItem(Character character) {
        for (int i = 0; i < _characterItems.Count; i++) {
            CharacterNameplateItem item = _characterItems[i];
            if (item.obj == character) {
                return item;
            }
        }
        return null;
    }
    private void OrderCharacterItems() {
        if (activeFaction.leader != null && activeFaction.leader is Character leader) {
            CharacterNameplateItem leaderItem = GetItem(leader);
            if (leaderItem == null) {
                throw new System.Exception($"Leader item in {activeFaction.name}'s UI is null! Leader is {leader.name}");
            }
            leaderItem.transform.SetAsFirstSibling();
        }
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        //Angels should not show in the characters list of faction in UI
        //https://trello.com/c/SGow0hA0/2234-angels-on-list
        if (FactionInfoHubUI.Instance.IsShowing(faction) && character.race != RACE.ANGEL) {
            if (GetCharacterItem(character) == null) {
                CreateNewCharacterItem(character);
            }
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        if (FactionInfoHubUI.Instance.IsShowing(faction)) {
            CharacterNameplateItem item = GetItem(character);
            if (item != null) {
                _characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item);
                //OrderCharacterItems();
            }
        }
    }
    private bool ShouldCharacterNameplateBeShown(Character character) {
        return IsCharacterFilteredByTraits(character) && IsCharacterFilteredByRegion(character) && !IsCharacterFilteredByDeath(character);
    }
    private bool IsCharacterFilteredByTraits(Character character) {
        //If character has one of the filtered traits, it means that the nameplate item of the character must show
        //The list of the filtered traits contains the traits that the player does want to show
        if(filteredTraits.Count <= 0) {
            return true;
        }
        for (int i = 0; i < filteredTraits.Count; i++) {
            if (character.traitContainer.HasTrait(filteredTraits[i]) && character.isInfoUnlocked) {
                return true;
            }
        }
        return false;
    }
    private bool IsCharacterFilteredByRegion(Character character) {
        //If character's home region is in one of the filtered regions, it means that the nameplate item of the character must show
        //The list of the filtered regions contains the traits that the player does want to show
        if (filteredVillages.Count <= 0) {
            return true;
        }
        for (int i = 0; i < filteredVillages.Count; i++) {
            if (character.homeSettlement == filteredVillages[i] && character.isInfoUnlocked) {
                return true;
            }
        }
        return false;
    }
    private bool IsCharacterFilteredByDeath(Character character) {
        //If alive toggle is on, this means that the player only wants to show those who are alive
        //So if it is on, and it returns false, that means that the character is alive and will be shown
        if (aliveToggle.isOn) {
            return character.isDead;
        }
        return false;
    }
    private void PopulateFilterTraits() {
        //traitFilterHalfCount = Mathf.RoundToInt(TraitManager.Instance.unhiddenTraitsNotStatuses.Count * 0.5f);
        for (int i = 0; i < TraitManager.Instance.unhiddenTraitsNotStatuses.Count; i++) {
            string traitName = TraitManager.Instance.unhiddenTraitsNotStatuses[i];
            CreateFactionTraitFilterItem(traitName);
        }
    }
    private void PopulateFilterRegions() {
        if (activeFaction != null) {
            ClearFilteredRegions();
            for (int i = 0; i < GridMap.Instance.mainRegion.settlementsInRegion.Count; i++) {
                if (GridMap.Instance.mainRegion.settlementsInRegion[i].locationType == LOCATION_TYPE.VILLAGE &&
                    GridMap.Instance.mainRegion.settlementsInRegion[i].owner == activeFaction) {
                    CreateFactionRegionFilterItem(GridMap.Instance.mainRegion.settlementsInRegion[i]);
                }
            }
        }
    }
    public void AddFilteredTrait(string traitName) {
        filteredTraits.Add(traitName);
        FilterCharacters();
    }
    public void RemoveFilteredTrait(string traitName) {
        if (filteredTraits.Remove(traitName)) {
            FilterCharacters();
        }
    }
    public void ClearFilteredTraits() {
        filteredTraits.Clear();
        FilterCharacters();
        ResetFilterTraits();
    }
    private void ResetFilterTraits() {
        for (int i = 0; i < _traitFilterItems.Count; i++) {
            _traitFilterItems[i].toggle.isOn = false;
        }
    }
    public void AddFilteredRegion(BaseSettlement village) {
        filteredVillages.Add(village);
        FilterCharacters();
    }
    public void RemoveFilteredRegion(BaseSettlement village) {
        if (filteredVillages.Remove(village)) {
            FilterCharacters();
        }
    }
    public void ClearFilteredRegions() {
        filteredVillages.Clear();
        filteredRegions.Clear();
        FilterCharacters();
        ResetFilterRegions();
    }
    private void ResetFilterRegions() {
        for (int i = 0; i < _regionFilterItems.Count; i++) {
            ObjectPoolManager.Instance.DestroyObject(_regionFilterItems[i]);
            _regionFilterItems[i].toggle.isOn = false;
        }
    }
    private void OnSearchTraitFilterValueChanged(string text) {
        if (string.IsNullOrEmpty(text)) {
            ResetSearchFilterTraits();
        }
    }
    private void OnSearchRegionFilterValueChanged(string text) {
        if (string.IsNullOrEmpty(text)) {
            ResetSearchFilterRegions();
        }
    }
    private void SearchFilterTraits(string text) {
        for (int i = 0; i < _traitFilterItems.Count; i++) {
            FactionTraitFilterItem item = _traitFilterItems[i];
            item.gameObject.SetActive(item.nameLbl.text.CaseInsensitiveContains(text));
        }
    }
    private void ResetSearchFilterTraits() {
        for (int i = 0; i < _traitFilterItems.Count; i++) {
            FactionTraitFilterItem item = _traitFilterItems[i];
            item.gameObject.SetActive(true);
        }
    }
    private void SearchFilterRegions(string text) {
        for (int i = 0; i < _regionFilterItems.Count; i++) {
            FactionRegionFilterItem item = _regionFilterItems[i];
            item.gameObject.SetActive(item.nameLbl.text.CaseInsensitiveContains(text));
        }
    }
    private void ResetSearchFilterRegions() {
        for (int i = 0; i < _regionFilterItems.Count; i++) {
            FactionRegionFilterItem item = _regionFilterItems[i];
            item.gameObject.SetActive(true);
        }
    }
    private FactionTraitFilterItem CreateFactionTraitFilterItem(string traitName) {
        Transform content = traitFilterScrollRect.content;
        //if(_traitFilterItems.Count > traitFilterHalfCount) {
        //    content = traitFilterColumn2ScrollRect.content;
        //}
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(traitFilterItemPrefab.name, Vector3.zero, Quaternion.identity, content);
        FactionTraitFilterItem item = go.GetComponent<FactionTraitFilterItem>();
        item.SetTraitName(traitName);
        _traitFilterItems.Add(item);
        return item;
    }
    private FactionRegionFilterItem CreateFactionRegionFilterItem(BaseSettlement village) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(regionFilterItemPrefab.name, Vector3.zero, Quaternion.identity, regionFilterScrollRect.content);
        FactionRegionFilterItem item = go.GetComponent<FactionRegionFilterItem>();
        item.SetVillage(village);
        _regionFilterItems.Add(item);
        return item;
    }
    public void OnClickSearchTraitFilter() {
        string text = searchTraitFilterField.text;
        SearchFilterTraits(text);
    }
    public void OnClickSearchRegionFilter() {
        string text = searchRegionFilterField.text;
        SearchFilterRegions(text);
    }
    public void OnToggleAlive(bool state) {
        FilterCharacters();
    }
    #endregion

    public void RevealInfo() {
        if (PlayerManager.Instance.player.plagueComponent.plaguePoints >= EditableValuesManager.Instance.GetRevealFactionInfoCost()) {
            PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-EditableValuesManager.Instance.GetRevealFactionInfoCost());
            activeFaction.isInfoUnlocked = true;
            ProcessDisplay();
        }
    }

    void ProcessDisplay() {
        switch (m_viewMode) {
            case VIEW_MODE.Overview:
            OnToggleOverview(true);
            break;
            case VIEW_MODE.Members:
            OnToggleMembers(true);
            break;
            case VIEW_MODE.Relations:
            OnToggleRelations(true);
            break;
            case VIEW_MODE.Crimes:
            OnToggleCrimes(true);
            break;
            case VIEW_MODE.Logs:
            OnToggleLogs(true);
            break;
        }
    }

    #region Utilities
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        crimesScrollRect.verticalNormalizedPosition = 1;
        //locationsScrollView.verticalNormalizedPosition = 1;
        logsWindow.ResetScrollPosition();
    }
    public void ShowFactionTestingInfo() {
        string summary = $"Faction Type: {activeFaction.factionType.type.ToString()}";
        for (int i = 0; i < activeFaction.ideologyComponent.currentIdeologies.Count; i++) {
            FactionIdeology ideology = activeFaction.ideologyComponent.currentIdeologies[i];
            if (ideology != null) {
                summary += $"\n{ideology.name}";
                summary += "\nRequirements for joining:";
                summary += $"\n\t{ideology.GetRequirementsForJoiningAsString()}";    
            }
        }
        summary += $"\n{name} Faction Job Queue:";
        if (activeFaction.availableJobs.Count > 0) {
            for (int j = 0; j < activeFaction.availableJobs.Count; j++) {
                JobQueueItem jqi = activeFaction.availableJobs[j];
                if (jqi is GoapPlanJob gpj) {
                    summary += $"\n<b>{gpj.name} Targeting {gpj.targetPOI?.ToString() ?? "None"}</b>" ;
                } else {
                    summary += $"\n<b>{jqi.name}</b>";
                }
                summary += $"\n Assigned Character: {jqi.assignedCharacter?.name}";
            }
        } else {
            summary += "\nNone";
        }

        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideFactionTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    private void InitializeRevealHoverText() {
        if (PlayerManager.Instance.player == null) {
            return;
        }
        if (PlayerManager.Instance.player.plagueComponent.plaguePoints < EditableValuesManager.Instance.GetRevealFactionInfoCost()) {
            btnRevealInfo.GetComponent<HoverText>()?.SetText("Not Enough Chaotic Energy");
            btnRevealInfo.GetComponent<RuinarchButton>().MakeUnavailable();
        } else {
            btnRevealInfo.GetComponent<HoverText>()?.SetText("Reveal Faction Info");
            btnRevealInfo.GetComponent<RuinarchButton>().MakeAvailable();
        }
    }

    #region toggles tabs
    public void OnToggleOverview(bool isOn) {
        HideAllScrollView();
        if (isOn) {
            InitializeRevealHoverText();
            m_viewMode = VIEW_MODE.Overview;
            if (activeFaction.isInfoUnlocked) {
                overviewScrollRectParent.SetActive(true);
                btnRevealInfo.SetActive(false);
            } else {
                overviewScrollRectParent.SetActive(false);
                btnRevealInfo.SetActive(true);
            }
        } else {
            overviewScrollRectParent.SetActive(false);
        }
    }

    public void OnToggleMembers(bool isOn) {
        HideAllScrollView();
        if (isOn) {
            InitializeRevealHoverText();
            m_viewMode = VIEW_MODE.Members;
            if (activeFaction.isInfoUnlocked) {
                membersScrollRectParent.SetActive(true);
                btnRevealInfo.SetActive(false);
            } else {
                membersScrollRectParent.SetActive(false);
                btnRevealInfo.SetActive(true);
            }
        } else {
            membersScrollRectParent.SetActive(false);
        }
    }

    public void OnToggleRelations(bool isOn) {
        HideAllScrollView();
        if (isOn) {
            InitializeRevealHoverText();
            m_viewMode = VIEW_MODE.Relations;
            if (activeFaction.isInfoUnlocked) {
                relationshipScrollRectParent.SetActive(true);
                btnRevealInfo.SetActive(false);
            } else {
                relationshipScrollRectParent.SetActive(false);
                btnRevealInfo.SetActive(true);
            }
        } else {
            relationshipScrollRectParent.SetActive(false);
        }
    }

    public void OnToggleCrimes(bool isOn) {
        HideAllScrollView();
        if (isOn) {
            InitializeRevealHoverText();
            m_viewMode = VIEW_MODE.Crimes;
            if (activeFaction.isInfoUnlocked) {
                crimesScrollRectParent.SetActive(true);
                btnRevealInfo.SetActive(false);
            } else {
                crimesScrollRectParent.SetActive(false);
                btnRevealInfo.SetActive(true);
            }
        } else {
            crimesScrollRectParent.SetActive(false);
        }
    }

    public void OnToggleLogs(bool isOn) {
        HideAllScrollView();
        if (isOn) {
            InitializeRevealHoverText();
            m_viewMode = VIEW_MODE.Logs;
            if (activeFaction.isInfoUnlocked) {
                logScrollRectParent.SetActive(true);
                btnRevealInfo.SetActive(false);
            } else {
                logScrollRectParent.SetActive(false);
                btnRevealInfo.SetActive(true);
            }
        } else {
            logScrollRectParent.SetActive(false);
        }
    }

    void HideAllScrollView() {
        overviewScrollRectParent.SetActive(false);
        membersScrollRectParent.SetActive(false);
        relationshipScrollRectParent.SetActive(false);
        crimesScrollRectParent.SetActive(false);
        logScrollRectParent.SetActive(false);
    }
    #endregion

    #region History
    private void UpdateHistory(Log log) {
        if (activeFaction != null && log.IsInvolved(activeFaction)) {
            UpdateAllHistoryInfo();
        }
    }
    public void UpdateAllHistoryInfo() {
        logsWindow.UpdateAllHistoryInfo();
    }
    #endregion   
}
