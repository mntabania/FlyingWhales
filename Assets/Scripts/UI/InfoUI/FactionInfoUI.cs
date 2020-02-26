using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using UnityEngine.UI.Extensions;
using EZObjectPools;

public class FactionInfoUI : InfoUIBase {

    private const int MAX_HISTORY_LOGS = 60;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private TextMeshProUGUI factionTypeLbl;
    [SerializeField] private FactionEmblem emblem;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    private List<CharacterNameplateItem> _characterItems;

    [Space(10)]
    [Header("Regions")]
    [SerializeField] private ScrollRect regionsScrollView;
    [SerializeField] private GameObject regionNameplatePrefab;
    private List<RegionNameplateItem> locationItems;

    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private RectTransform relationshipsParent;
    [SerializeField] private GameObject relationshipPrefab;
    
    internal Faction currentlyShowingFaction => _data as Faction;
    private Faction activeFaction { get; set; }

    internal override void Initialize() {
        base.Initialize();
        _characterItems = new List<CharacterNameplateItem>();
        locationItems = new List<RegionNameplateItem>();
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Faction, Settlement>(Signals.FACTION_OWNED_REGION_ADDED, OnFactionRegionAdded);
        Messenger.AddListener<Faction, Settlement>(Signals.FACTION_OWNED_REGION_REMOVED, OnFactionRegionRemoved);
        Messenger.AddListener<FactionRelationship>(Signals.FACTION_RELATIONSHIP_CHANGED, OnFactionRelationshipChanged);
        Messenger.AddListener<Faction>(Signals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
    }

    public override void OpenMenu() {
        Faction previousArea = activeFaction;
        activeFaction = _data as Faction;
        base.OpenMenu();
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        UpdateFactionInfo();
        UpdateAllCharacters();
        UpdateRegions();
        UpdateAllRelationships();
        ResetScrollPositions();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        activeFaction = null;
    }

    public void UpdateFactionInfo() {
        if (activeFaction == null) {
            return;
        }
        UpdateBasicInfo();
        //ResetScrollPositions();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        factionNameLbl.text = activeFaction.name;
        factionTypeLbl.text = activeFaction.GetRaceText();
        emblem.SetFaction(activeFaction);
    }
    #endregion

    #region Characters
    private void UpdateAllCharacters() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        _characterItems.Clear();

        for (int i = 0; i < activeFaction.characters.Count; i++) {
            Character currCharacter = activeFaction.characters[i];
            CreateNewCharacterItem(currCharacter, false);
        }
        OrderCharacterItems();
    }
    private LandmarkCharacterItem GetItem(Party party) {
        LandmarkCharacterItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<LandmarkCharacterItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            LandmarkCharacterItem item = items[i];
            if (item.character != null) {
                if (item.character.ownParty.id == party.id) {
                    return item;
                }
            }
        }
        return null;
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
        _characterItems.Add(item);
        if (autoSort) {
            OrderCharacterItems();
        }
        return item;
    }
    private void OrderCharacterItems() {
        if (activeFaction.leader != null && activeFaction.leader is Character) {
            Character leader = activeFaction.leader as Character;
            CharacterNameplateItem leaderItem = GetItem(leader);
            if (leaderItem == null) {
                throw new System.Exception($"Leader item in {activeFaction.name}'s UI is null! Leader is {leader.name}");
            }
            leaderItem.transform.SetAsFirstSibling();
        }
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if (isShowing && activeFaction.id == faction.id) {
            CreateNewCharacterItem(character);
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        if (isShowing && activeFaction != null && activeFaction.id == faction.id) {
            CharacterNameplateItem item = GetItem(character);
            if (item != null) {
                _characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item);
                OrderCharacterItems();
            }
        }
    }
    #endregion

    #region Regions
    private void UpdateRegions() {
        UtilityScripts.Utilities.DestroyChildren(regionsScrollView.content);
        locationItems.Clear();

        //TODO:
        // for (int i = 0; i < activeFaction.ownedSettlements.Count; i++) {
        //     Region currRegion = activeFaction.ownedSettlements[i];
        //     CreateNewRegionItem(currRegion);
        // }
    }
    private void CreateNewRegionItem(Region region) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(regionNameplatePrefab.name, regionsScrollView.content);
        RegionNameplateItem item = characterGO.GetComponent<RegionNameplateItem>();
        item.SetObject(region);
        locationItems.Add(item);
    }
    private RegionNameplateItem GetLocationItem(Region region) {
        for (int i = 0; i < locationItems.Count; i++) {
            RegionNameplateItem locationPortrait = locationItems[i];
            if (locationPortrait.obj.id == region.id) {
                return locationPortrait;
            }
        }
        return null;
    }
    private void DestroyLocationItem(Region region) {
        RegionNameplateItem item = GetLocationItem(region);
        if (item != null) {
            locationItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }
    private void OnFactionRegionAdded(Faction faction, Settlement region) {
        if (isShowing && activeFaction.id == faction.id) {
            //TODO:
            // CreateNewRegionItem(region);
        }
    }
    private void OnFactionRegionRemoved(Faction faction, Settlement region) {
        if (isShowing && activeFaction.id == faction.id) {
            //TODO:
            // DestroyLocationItem(region);
        }
    }
    #endregion

    #region Relationships
    private void UpdateAllRelationships() {
        UtilityScripts.Utilities.DestroyChildren(relationshipsParent);

        foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in activeFaction.relationships) {
            if (keyValuePair.Key.isActive) {
                GameObject relGO = UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsParent);
                FactionRelationshipItem item = relGO.GetComponent<FactionRelationshipItem>();
                item.SetData(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
    private void OnFactionRelationshipChanged(FactionRelationship rel) {
        if (isShowing && (rel.faction1.id == activeFaction.id || rel.faction2.id == activeFaction.id)) {
            UpdateAllRelationships();
        }
    }
    private void OnFactionActiveChanged(Faction faction) {
        if (isShowing) {
            UpdateAllRelationships();
        }
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        regionsScrollView.verticalNormalizedPosition = 1;
    }
    private void OnInspectAll() {
        if (isShowing && activeFaction != null) {
            UpdateAllCharacters();
            //UpdateHiddenUI();
        }
    }
    public void ShowFactionTestingInfo() {
        string summary = string.Empty;
        if (activeFaction.activeQuest != null) {
            summary += activeFaction.activeQuest.name;
            for (int i = 0; i < activeFaction.activeQuest.availableJobs.Count; i++) {
                JobQueueItem item = activeFaction.activeQuest.availableJobs[i];
                summary += $"\n\t- {item.jobType}: {item.assignedCharacter?.name}";
            }
        }
        //if (!string.IsNullOrEmpty(summary)) {
        //    summary += "\n";
        //}
        for (int i = 0; i < activeFaction.ideologyComponent.currentIdeologies.Length; i++) {
            FactionIdeology ideology = activeFaction.ideologyComponent.currentIdeologies[i];
            summary += $"\n{ideology.name}";
            summary += "\nRequirements for joining:";
            summary += $"\n\t{ideology.GetRequirementsForJoiningAsString()}";
        }

        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideFactionTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    private void OnOpenShareIntelMenu() {
        backButton.interactable = false;
    }
    private void OnCloseShareIntelMenu() { }
}
