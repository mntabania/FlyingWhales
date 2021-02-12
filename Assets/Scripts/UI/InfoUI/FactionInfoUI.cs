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

public class FactionInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private TextMeshProUGUI factionTypeLbl;
    [SerializeField] private FactionEmblem emblem;

    [Space(10)]
    [Header("Overview")]
    [SerializeField] private TextMeshProUGUI overviewFactionNameLbl;
    [SerializeField] private TextMeshProUGUI overviewFactionTypeLbl;
    [SerializeField] private CharacterNameplateItem leaderNameplateItem;
    [SerializeField] private TextMeshProUGUI ideologyLbl;
    
    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    private List<CharacterNameplateItem> _characterItems;

    [Space(10)]
    [Header("Regions")]
    [SerializeField] private ScrollRect locationsScrollView;
    [SerializeField] private GameObject settlementNameplatePrefab;
    private List<SettlementNameplateItem> locationItems;

    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private RectTransform relationshipsParent;
    [SerializeField] private GameObject relationshipPrefab;

    [Space(10)] [Header("Logs")] 
    [SerializeField] private LogsWindow logsWindow;
    
    internal Faction currentlyShowingFaction => _data as Faction;
    private Faction activeFaction { get; set; }

    internal override void Initialize() {
        base.Initialize();
        _characterItems = new List<CharacterNameplateItem>();
        locationItems = new List<SettlementNameplateItem>();
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_ADDED, OnFactionSettlementAdded);
        Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_REMOVED, OnFactionSettlementRemoved);
        Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionRelationshipChanged);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
        Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnFactionLeaderChanged);
        Messenger.AddListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_IDEOLOGIES_CHANGED, OnFactionIdeologiesChanged);
        logsWindow.Initialize();
    }
    public override void OpenMenu() {
        Faction previousArea = activeFaction;
        activeFaction = _data as Faction;
        base.OpenMenu();
        if (UIManager.Instance.IsConversationMenuOpen()) {
            backButton.interactable = false;
        }
        UpdateOverview();
        UpdateFactionInfo();
        UpdateAllCharacters();
        UpdateOwnedLocations();
        UpdateAllRelationships();
        logsWindow.OnParentMenuOpened(activeFaction.persistentID);
        UpdateAllHistoryInfo();
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
        factionNameLbl.text = activeFaction.nameWithColor;
        factionTypeLbl.text = activeFaction.GetRaceText();
        emblem.SetFaction(activeFaction);
    }
    #endregion

    #region Characters
    private void UpdateAllCharacters() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        _characterItems.Clear();

        //Angels should not show in the characters list of faction in UI
        //https://trello.com/c/SGow0hA0/2234-angels-on-list
        for (int i = 0; i < activeFaction.characters.Count; i++) {
            Character currCharacter = activeFaction.characters[i];
            if(currCharacter.race != RACE.ANGEL) {
                CreateNewCharacterItem(currCharacter, false);
            }
        }
        OrderCharacterItems();
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
        if (isShowing && activeFaction.id == faction.id && character.race != RACE.ANGEL) {
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
    private void UpdateOwnedLocations() {
        UtilityScripts.Utilities.DestroyChildren(locationsScrollView.content);
        locationItems.Clear();
        for (int i = 0; i < activeFaction.ownedSettlements.Count; i++) {
            BaseSettlement ownedSettlement = activeFaction.ownedSettlements[i];
            CreateNewSettlementItem(ownedSettlement);
        }
    }
    private void CreateNewSettlementItem(BaseSettlement settlement) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(settlementNameplatePrefab.name, locationsScrollView.content);
        SettlementNameplateItem item = characterGO.GetComponent<SettlementNameplateItem>();
        item.SetObject(settlement);
        item.SetAsButton();
        item.AddOnClickAction(OnClickSettlementItem);
        locationItems.Add(item);
    }
    private void OnClickSettlementItem(BaseSettlement settlement) {
        if (settlement.areas.Count > 0) {
            Area tile = settlement.areas[0];
            if (InnerMapManager.Instance.isAnInnerMapShowing) {
                //if inner map is showing, open inner map of hextile then center on it
                if (InnerMapManager.Instance.currentlyShowingLocation != tile.region) {
                    InnerMapManager.Instance.TryShowLocationMap(tile.region);    
                }
                InnerMapCameraMove.Instance.CenterCameraOnTile(tile);
            } 
            //else {
            //    //if world map is showing, just center on hextile
            //    tile.CenterCameraHere();
            //}
            //UIManager.Instance.ShowHexTileInfo(tile);
        }
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
        if (isShowing && activeFaction.id == faction.id) {
            CreateNewSettlementItem(settlement);
        }
    }
    private void OnFactionSettlementRemoved(Faction faction, BaseSettlement settlement) {
        if (isShowing && activeFaction.id == faction.id) {
            DestroyLocationItem(settlement);
        }
    }
    #endregion

    #region Relationships
    private void UpdateAllRelationships() {
        UtilityScripts.Utilities.DestroyChildren(relationshipsParent);

        foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in activeFaction.relationships) {
            if (keyValuePair.Key.isActive) {
                if(keyValuePair.Key.factionType.type == FACTION_TYPE.Undead && keyValuePair.Key.leader == null) {
                    //Only add Undead faction in Relations once it gains a Faction Leader
                    continue;
                }
                GameObject relGO = UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsParent);
                FactionRelationshipItem item = relGO.GetComponent<FactionRelationshipItem>();
                item.SetData(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
    private void OnFactionRelationshipChanged(Faction faction1, Faction faction2, FACTION_RELATIONSHIP_STATUS newStatus, FACTION_RELATIONSHIP_STATUS oldStatus) {
        if (isShowing && (faction1.id == activeFaction.id || faction2.id == activeFaction.id)) {
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
        locationsScrollView.verticalNormalizedPosition = 1;
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

    #region Overview
    private void OnFactionLeaderChanged(Character character, ILeader previousLeader) {
        if (isShowing) {
            UpdateOverview();
        }
    }
    private void OnFactionIdeologiesChanged(Faction faction) {
        if (isShowing && faction == activeFaction) {
            UpdateOverview();
        }
    }
    private void OnFactionLeaderRemoved(Faction faction, ILeader previousLeader) {
        if (isShowing && faction == activeFaction) {
            UpdateOverview();
        }
    }
    private void UpdateOverview() {
        overviewFactionNameLbl.text = activeFaction.nameWithColor;
        overviewFactionTypeLbl.text = activeFaction.factionType.name;

        if (activeFaction.leader is Character leader) {
            leaderNameplateItem.gameObject.SetActive(true);
            leaderNameplateItem.SetObject(leader);
        } else {
            leaderNameplateItem.gameObject.SetActive(false);
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
    
    #region History
    private void UpdateHistory(Log log) {
        if (isShowing && log.IsInvolved(activeFaction)) {
            UpdateAllHistoryInfo();
        }
    }
    public void UpdateAllHistoryInfo() {
        logsWindow.UpdateAllHistoryInfo();
    }
    #endregion   
}
