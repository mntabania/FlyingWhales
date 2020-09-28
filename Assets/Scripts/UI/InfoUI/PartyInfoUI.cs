using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Inner_Maps;
using Traits;
using Locations.Settlements;

public class PartyInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI questLbl;
    [SerializeField] private TextMeshProUGUI noHomeSettlementLbl;
    [SerializeField] private SettlementNameplateItem homeSettlementNameplate;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private Toggle membersToggle;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect membersScrollView;
    [SerializeField] private GameObject membersGO;

    [Space(10)] [Header("Logs")] 
    [SerializeField] private LogsWindow logsWindow;

    public Party activeParty { get; private set; }

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Log>(Signals.LOG_ADDED, UpdateLogsFromSignal);
        Messenger.AddListener<Log>(Signals.LOG_IN_DATABASE_UPDATED, UpdateLogsFromSignal);
        Messenger.AddListener<Party, Character>(Signals.CHARACTER_JOINED_PARTY, UpdateMembersFromSignal);
        Messenger.AddListener<Party, Character>(Signals.CHARACTER_LEFT_PARTY, UpdateMembersFromSignal);
        Messenger.AddListener<Party>(Signals.DISBAND_PARTY, UpdateMembersFromSignal);

        homeSettlementNameplate.SetAsButton();
        homeSettlementNameplate.ClearAllOnClickActions();
        homeSettlementNameplate.AddOnClickAction(OnClickSettlementItem);

        logsWindow.Initialize();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        activeParty = null;
    }
    public override void OpenMenu() {
        activeParty = _data as Party;
        base.OpenMenu();
        UIManager.Instance.HideObjectPicker();
        UpdateTabs();
        UpdateBasicInfo();
        UpdateInfo();
        UpdateMembers();
        logsWindow.OnParentMenuOpened(activeParty.persistentID);
        UpdateLogs();
    }
    #endregion

    #region General
    public void UpdatePartyInfo() {
        if(activeParty == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateInfo();
    }
    private void UpdateTabs() {
        if (activeParty.members != null && activeParty.members.Count > 0) {
            membersToggle.interactable = true;
        } else {
            membersToggle.isOn = false;
            membersToggle.interactable = false;
        }
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeParty.partyName;
    }
    private void UpdateInfo() {
        string questText = "No Quest";
        if (activeParty.isActive) {
            questText = activeParty.currentQuest.GetPartyQuestTextInLog();
        }
        questLbl.text = $"{questText}";

        if(activeParty.partySettlement != null) {
            if(homeSettlementNameplate.obj != activeParty.partySettlement) {
                homeSettlementNameplate.SetObject(activeParty.partySettlement);
            }
            homeSettlementNameplate.gameObject.SetActive(true);
            noHomeSettlementLbl.gameObject.SetActive(false);
        } else {
            homeSettlementNameplate.gameObject.SetActive(false);
            noHomeSettlementLbl.gameObject.SetActive(true);
        }
    }
    private void UpdateMembers() {
        UtilityScripts.Utilities.DestroyChildren(membersScrollView.content);
        List<Character> members = activeParty.members;
        if (members != null && members.Count > 0) {
            for (int i = 0; i < members.Count; i++) {
                Character character = members[i];
                if(character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, membersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
    public void UpdateLogs() {
        logsWindow.UpdateAllHistoryInfo();
    }
    #endregion

    #region Listeners
    private void UpdateLogsFromSignal(Log log) {
        if(isShowing && log.IsInvolved(activeParty)) {
            UpdateLogs();
        }
    }
    private void UpdateMembersFromSignal(Party party, Character member) {
        if (isShowing && activeParty == party) {
            UpdateMembers();
        }
    }
    private void UpdateMembersFromSignal(Party party) {
        if (isShowing && activeParty == party) {
            UpdateMembers();
        }
    }
    #endregion

    #region Click Actions
    private void OnClickSettlementItem(BaseSettlement settlement) {
        if (settlement.tiles.Count > 0) {
            HexTile tile = settlement.tiles[0];
            if (InnerMapManager.Instance.isAnInnerMapShowing) {
                //if inner map is showing, open inner map of hextile then center on it
                if (InnerMapManager.Instance.currentlyShowingLocation != tile.region) {
                    InnerMapManager.Instance.TryShowLocationMap(tile.region);
                }
                InnerMapCameraMove.Instance.CenterCameraOnTile(tile);
            } else {
                //if world map is showing, just center on hextile
                tile.CenterCameraHere();
            }
            UIManager.Instance.ShowHexTileInfo(tile);
        }
    }
    #endregion
}
