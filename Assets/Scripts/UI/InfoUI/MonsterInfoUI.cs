using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using TMPro;
using UnityEngine.UI;
using Traits;
using UnityEngine.Serialization;

public class MonsterInfoUI : InfoUIBase {
    
    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI plansLbl;
    [SerializeField] private LogItem plansLblLogItem;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private UIHoverPosition logHoverPosition;
    private LogHistoryItem[] logHistoryItems;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;
    [SerializeField] private TextMeshProUGUI raceLbl;
    [SerializeField] private TextMeshProUGUI elementLbl;

    [Space(10)]
    [Header("Traits")]
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private TextMeshProUGUI itemsLbl;
    
    private Character _activeMonster;

    public Character activeMonster => _activeMonster;
    private List<string> combatModes;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<IPointOfInterest>(Signals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<Character>(Signals.UPDATE_THOUGHT_BUBBLE, UpdateThoughtBubbleFromSignal);
        
        InitializeLogsMenu();

        ConstructCombatModes();
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        if (_activeMonster != null && ReferenceEquals(_activeMonster.marker, null) == false) {
            if (InnerMapCameraMove.Instance.target == _activeMonster.marker.gameObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
            _activeMonster.marker.UpdateNameplateElementsState();
        }
        _activeMonster = null;
    }
    public override void OpenMenu() {
        Character previousMonster = _activeMonster;
        _activeMonster = _data as Character;
        base.OpenMenu();
        if (previousMonster != null && previousMonster.marker != null) {
            previousMonster.marker.UpdateNameplateElementsState();
        }
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        if (_activeMonster.marker && _activeMonster.marker.transform != null) {
            Selector.Instance.Select(_activeMonster, _activeMonster.marker.transform);
            _activeMonster.marker.UpdateNameplateElementsState();
        }
        UpdateMonsterInfo();
        UpdateTraits();
        UpdateInventoryInfo();
        UpdateHistory(_activeMonster);
        ResetAllScrollPositions();
    }
    protected override void OnExecutePlayerAction(PlayerAction action) {
        base.OnExecutePlayerAction(action);
        if(action.type == SPELL_TYPE.CHANGE_COMBAT_MODE) {
            SetCombatModeUIPosition(action);
        }
    }
    protected override void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = PlayerSkillManager.Instance.GetPlayerActionData(target.actions[i]);
            if (action.IsValid(target) && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(action.type)) {
                //if (action.actionName == PlayerDB.Combat_Mode_Action) {
                //    action.SetLabelText(action.actionName + ": " + UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeCharacter.combatComponent.combatMode.ToString()));
                //}
                ActionItem actionItem = AddNewAction(action, target);
                actionItem.SetInteractable(action.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
            }
        }
    }
    #endregion

    #region Utilities
    private void InitializeLogsMenu() {
        logHistoryItems = new LogHistoryItem[CharacterManager.MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < CharacterManager.MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
    }
    private void ResetAllScrollPositions() {
        historyScrollView.verticalNormalizedPosition = 1;
    }
    public void UpdateMonsterInfo() {
        if (_activeMonster == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeMonster);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeMonster.visuals.GetNameplateName();
        lvlClassLbl.text = _activeMonster.raceClassName;
        UpdateThoughtBubble();
    }
    public void UpdateThoughtBubble() {
        plansLbl.text = activeMonster.visuals.GetThoughtBubble(out var log);
        if (log != null) {
            plansLblLogItem.SetLog(log);
        }
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = $"{_activeMonster.currentHP.ToString()}/{_activeMonster.maxHP.ToString()}";
        attackLbl.text = $"{_activeMonster.combatComponent.attack.ToString()}";
        speedLbl.text = $"{_activeMonster.combatComponent.attackSpeed / 1000f}s";
        raceLbl.text = $"{UtilityScripts.GameUtilities.GetNormalizedSingularRace(_activeMonster.race)}";
        elementLbl.text = $"{_activeMonster.combatComponent.elementalDamage.type.ToString()}";
        //if(characterPortrait.character != null) {
        //    characterPortrait.UpdateLvl();
        //}
    }
    #endregion

    #region Traits
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(!isShowing || _activeMonster != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
    }
    private void UpdateThoughtBubbleFromSignal(Character character) {
        if (isShowing && _activeMonster == character) {
            UpdateThoughtBubble();
        }
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;

        for (int i = 0; i < _activeMonster.traitContainer.statuses.Count; i++) {
            Status currStatus = _activeMonster.traitContainer.statuses[i];
            if (currStatus.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (!string.IsNullOrEmpty(statusTraits)) {
                statusTraits = $"{statusTraits}, ";
            }
            statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currStatus.GetNameInUI(_activeMonster)}</link></color></b>";
        }
        for (int i = 0; i < _activeMonster.traitContainer.traits.Count; i++) {
            Trait currTrait = _activeMonster.traitContainer.traits[i];
            if (currTrait.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (currTrait.type == TRAIT_TYPE.BUFF) {
                color = UIManager.buffTextColor;
            } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                color = UIManager.flawTextColor;
            }
            if (!string.IsNullOrEmpty(normalTraits)) {
                normalTraits = $"{normalTraits}, ";
            }
            normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(_activeMonster)}</link></color></b>";
        }

        statusTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(statusTraits) == false) {
            //character has status traits
            statusTraitsLbl.text = statusTraits; 
        }
        normalTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(normalTraits) == false) {
            //character has normal traits
            normalTraitsLbl.text = normalTraits;
        }
    }
    public void OnHoverTrait(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeMonster.traitContainer.traits[index];
            UIManager.Instance.ShowSmallInfo(trait.description);
        }
    }
    public void OnHoverStatus(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeMonster.traitContainer.statuses[index];
            UIManager.Instance.ShowSmallInfo(trait.description);
        }
    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(TileObject item, Character character) {
        if (isShowing && _activeMonster == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        itemsLbl.text = string.Empty;
        for (int i = 0; i < _activeMonster.items.Count; i++) {
            TileObject currInventoryItem = _activeMonster.items[i];
            itemsLbl.text = $"{itemsLbl.text}{currInventoryItem.name}";
            if (i < _activeMonster.items.Count - 1) {
                itemsLbl.text = $"{itemsLbl.text}, ";
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(IPointOfInterest poi) {
        if (isShowing && poi == _activeMonster) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        int historyCount = _activeMonster.logComponent.history.Count;
        int historyLastIndex = historyCount - 1;
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < historyCount) {
                Log currLog = _activeMonster.logComponent.history[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                currItem.SetHoverPosition(logHoverPosition);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
    }
    private void ClearHistory() {
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            currItem.gameObject.SetActive(false);
        }
    }
    #endregion   

    #region Listeners
    private void OnOpenShareIntelMenu() {
        backButton.interactable = false;
    }
    //private void OnCloseShareIntelMenu() { }
    private void OnCharacterDied(Character character) {
        if (this.isShowing && activeMonster == character) {
            InnerMapCameraMove.Instance.CenterCameraOn(null);
        }
    }
    #endregion

    #region For Testing
    public void ShowCharacterTestingInfo() {
#if UNITY_EDITOR
        string summary = $"Home structure: {activeMonster.homeStructure}" ?? "None";
        summary = $"{summary}{($"\nCurrent structure: {activeMonster.currentStructure}" ?? "None")}";
        summary = $"{summary}{("\nPOI State: " + activeMonster.state.ToString())}";
        summary = $"{summary}{("\nDo Not Get Hungry: " + activeMonster.needsComponent.doNotGetHungry)}";
        summary = $"{summary}{("\nDo Not Get Tired: " + activeMonster.needsComponent.doNotGetTired)}";
        summary = $"{summary}{("\nDo Not Get Bored: " + activeMonster.needsComponent.doNotGetBored)}";
        summary = $"{summary}{("\nDo Not Recover HP: " + activeMonster.doNotRecoverHP)}";
        summary = $"{summary}{("\nCan Move: " + activeMonster.canMove)}";
        summary = $"{summary}{("\nCan Witness: " + activeMonster.canWitness)}";
        summary = $"{summary}{("\nCan Be Attacked: " + activeMonster.canBeAttacked)}";
        summary = $"{summary}{("\nCan Perform: " + activeMonster.canPerform)}";
        //summary = $"{summary}{("\nIs Missing: " + activeMonster.isMissing)}";
        summary = $"{summary}{("\n" + activeMonster.needsComponent.GetNeedsSummary())}";
        summary = $"{summary}{("\nFullness Time: " + (activeMonster.needsComponent.fullnessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeMonster.needsComponent.fullnessForcedTick)))}";
        summary = $"{summary}{("\nTiredness Time: " + (activeMonster.needsComponent.tirednessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeMonster.needsComponent.tirednessForcedTick)))}";
        summary = $"{summary}{("\nRemaining Sleep Ticks: " + activeMonster.needsComponent.currentSleepTicks)}";
        summary = $"{summary}{("\nFood: " + activeMonster.food)}";
        // summary = $"{summary}{("\nRole: " + activeCharacter.role.roleType.ToString())}";
        summary = $"{summary}{("\nSexuality: " + activeMonster.sexuality.ToString())}";
        summary = $"{summary}{("\nMood: " + activeMonster.moodComponent.moodValue + "/100" + "(" + activeMonster.moodComponent.moodState.ToString() + ")")}";
        summary = $"{summary}{("\nHP: " + activeMonster.currentHP + "/" + activeMonster.maxHP)}";
        summary = $"{summary}{("\nAttack Range: " + activeMonster.characterClass.attackRange)}";
        summary = $"{summary}{("\nAttack Speed: " + activeMonster.combatComponent.attackSpeed)}";
        summary = $"{summary}{("\nCombat Mode: " + activeMonster.combatComponent.combatMode.ToString())}";
        summary = $"{summary}{("\nElemental Type: " + activeMonster.combatComponent.elementalDamage.name)}";

        if (activeMonster.stateComponent.currentState != null) {
            summary = $"{summary}\nCurrent State: {activeMonster.stateComponent.currentState}";
            summary = $"{summary}\n\tDuration in state: {activeMonster.stateComponent.currentState.currentDuration}/{activeMonster.stateComponent.currentState.duration}";
        }
        
        summary += "\nBehaviour Components: ";
        for (int i = 0; i < activeMonster.behaviourComponent.currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = activeMonster.behaviourComponent.currentBehaviourComponents[i];
            summary += $"{component.ToString()}, ";
        }
        
        summary += "\nPersonal Job Queue: ";
        if (activeMonster.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < activeMonster.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = activeMonster.jobQueue.jobsInQueue[i];
                summary += $"{poi}, ";
            }
        } else {
            summary += "None";
        }
        
        // summary += "\n" + activeCharacter.needsComponent.GetNeedsSummary();
        // summary += "\n\nAlter Egos: ";
        // for (int i = 0; i < activeCharacter.alterEgos.Values.Count; i++) {
        //     summary += "\n" + activeCharacter.alterEgos.Values.ElementAt(i).GetAlterEgoSummary();
        // }
        UIManager.Instance.ShowSmallInfo(summary);
#endif
    }
    public void HideCharacterTestingInfo() {
#if UNITY_EDITOR
        UIManager.Instance.HideSmallInfo();
#endif
        
    }
    #endregion

    #region Combat Modes
    private void ConstructCombatModes() {
        combatModes = new List<string>();
        for (int i = 0; i < CharacterManager.Instance.combatModes.Length; i++) {
            combatModes.Add(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(CharacterManager.Instance.combatModes[i].ToString()));
        }
    }
    public void ShowSwitchCombatModeUI() {
        UIManager.Instance.customDropdownList.ShowDropdown(combatModes, OnClickChooseCombatMode, CanChoostCombatMode);
    }
    private void SetCombatModeUIPosition(PlayerAction action) {
        ActionItem actionItem = GetActiveActionItem(action);
        if (actionItem != null) {
            Vector3 actionWorldPos = actionItem.transform.localPosition;
            UIManager.Instance.customDropdownList.SetPosition(new Vector3(actionWorldPos.x, actionWorldPos.y + 10f, actionWorldPos.z));
        }
    }
    private bool CanChoostCombatMode(string mode) {
        if(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeMonster.combatComponent.combatMode.ToString())
            == mode) {
            return false;
        }
        return true;
    }
    private void OnClickChooseCombatMode(string mode) {
        COMBAT_MODE combatMode = (COMBAT_MODE) System.Enum.Parse(typeof(COMBAT_MODE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(mode));
        UIManager.Instance.characterInfoUI.activeCharacter.combatComponent.SetCombatMode(combatMode);
        Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, activeMonster as IPlayerActionTarget);
        UIManager.Instance.customDropdownList.Close();
    }
    #endregion
}
