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
using Actionables;

public class MinionInfoUI : InfoUIBase {
    
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
    private LogHistoryItem[] logHistoryItems;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;

    [Space(10)]
    [Header("Traits")]
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private TextMeshProUGUI itemsLbl;
    
    private Minion _activeMinion;

    public Minion activeMinion => _activeMinion;
    private List<string> combatModes;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<IPointOfInterest>(Signals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        //Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
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
        if (_activeMinion != null && ReferenceEquals(_activeMinion.character.marker, null) == false 
            && InnerMapCameraMove.Instance.target == _activeMinion.character.marker.gameObject.transform) {
            InnerMapCameraMove.Instance.CenterCameraOn(null);
        }
        _activeMinion = null;
    }
    public override void OpenMenu() {
        _activeMinion = _data as Minion;
        base.OpenMenu();
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        if (_activeMinion.character.marker && _activeMinion.character.marker.transform != null) {
            Selector.Instance.Select(_activeMinion.character, _activeMinion.character.marker.transform);    
        }
        UpdateMinionInfo();
        UpdateTraits();
        UpdateInventoryInfo();
        UpdateHistory(_activeMinion.character);
        ResetAllScrollPositions();
    }
    protected override void OnPlayerActionExecuted(PlayerAction action) {
        base.OnPlayerActionExecuted(action);
        if(action.actionName == PlayerDB.Combat_Mode_Action) {
            SetCombatModeUIPosition(action);
        }
    }
    protected override void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = target.actions[i];
            if (action.IsValid(target) && PlayerManager.Instance.player.archetype.CanDoAction(action.actionName)) {
                if (action.actionName == PlayerDB.Combat_Mode_Action) {
                    action.SetLabelText(action.actionName + ": " + UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeMinion.character.combatComponent.combatMode.ToString()));
                }
                ActionItem actionItem = AddNewAction(action);
                actionItem.SetInteractable(action.isActionClickableChecker.Invoke() && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
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
    public void UpdateMinionInfo() {
        if (_activeMinion == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeMinion.character);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeMinion.character.visuals.GetNameplateName();
        lvlClassLbl.text = _activeMinion.character.raceClassName;
        UpdateThoughtBubble();
    }
    public void UpdateThoughtBubble() {
        plansLbl.text = activeMinion.character.visuals.GetThoughtBubble(out var log);
        if (log != null) {
            plansLblLogItem.SetLog(log);
        }
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = $"{_activeMinion.character.currentHP}/{_activeMinion.character.maxHP}";
        attackLbl.text = $"{_activeMinion.character.attackPower}";
        speedLbl.text = $"{_activeMinion.character.speed}";
        if(characterPortrait.character != null) {
            characterPortrait.UpdateLvl();
        }
    }
    #endregion

    #region Traits
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(!isShowing || _activeMinion.character != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
    }
    private void UpdateThoughtBubbleFromSignal(Character character) {
        if (isShowing && _activeMinion.character == character) {
            UpdateThoughtBubble();
        }
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;

        for (int i = 0; i < _activeMinion.character.traitContainer.allTraits.Count; i++) {
            Trait currTrait = _activeMinion.character.traitContainer.allTraits[i];
            if (currTrait.isHidden) {
                continue; //skip
            }
            if (currTrait.type == TRAIT_TYPE.ABILITY || currTrait.type == TRAIT_TYPE.ATTACK || currTrait.type == TRAIT_TYPE.COMBAT_POSITION
                || currTrait.name == "Herbivore" || currTrait.name == "Carnivore") {
                continue; //hide combat traits
            }
            if (currTrait.type == TRAIT_TYPE.STATUS || currTrait.type == TRAIT_TYPE.DISABLER || currTrait.type == TRAIT_TYPE.ENCHANTMENT || currTrait.type == TRAIT_TYPE.EMOTION) {
                string color = UIManager.normalTextColor;
                if (currTrait.type == TRAIT_TYPE.BUFF) {
                    color = UIManager.buffTextColor;
                } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                    color = UIManager.flawTextColor;
                }
                if (!string.IsNullOrEmpty(statusTraits)) {
                    statusTraits = $"{statusTraits}, ";
                }
                statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(activeMinion.character)}</link></color></b>";
            } else {
                string color = UIManager.normalTextColor;
                if (currTrait.type == TRAIT_TYPE.BUFF) {
                    color = UIManager.buffTextColor;
                } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                    color = UIManager.flawTextColor;
                }
                if (!string.IsNullOrEmpty(normalTraits)) {
                    normalTraits = $"{normalTraits}, ";
                }
                normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(activeMinion.character)}</link></color></b>";
            }
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
        if (obj is string) {
            string text = (string) obj;
            int index = int.Parse(text);
            Trait trait = activeMinion.character.traitContainer.allTraits[index];
            UIManager.Instance.ShowSmallInfo(trait.description);
        }

    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(TileObject item, Character character) {
        if (isShowing && _activeMinion.character == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        itemsLbl.text = string.Empty;
        for (int i = 0; i < _activeMinion.character.items.Count; i++) {
            TileObject currInventoryItem = _activeMinion.character.items[i];
            itemsLbl.text = $"{itemsLbl.text} {currInventoryItem.name}";
            if (i < _activeMinion.character.items.Count - 1) {
                itemsLbl.text = $"{itemsLbl.text}, ";
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(IPointOfInterest poi) {
        if (isShowing && poi == _activeMinion.character) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        int historyCount = _activeMinion.character.logComponent.history.Count;
        int historyLastIndex = historyCount - 1;
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < historyCount) {
                Log currLog = _activeMinion.character.logComponent.history[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
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
        if (this.isShowing && activeMinion.character == character) {
            InnerMapCameraMove.Instance.CenterCameraOn(null);
        }
    }
    #endregion

    #region For Testing
    public void ShowCharacterTestingInfo() {
        string summary = $"Home structure: {activeMinion.character.homeStructure}" ?? "None";
        summary = $"{summary}{($"\nCurrent structure: {activeMinion.character.currentStructure}" ?? "None")}";
        summary = $"{summary}{("\nPOI State: " + activeMinion.character.state.ToString())}";
        summary = $"{summary}{("\nDo Not Get Hungry: " + activeMinion.character.needsComponent.doNotGetHungry)}";
        summary = $"{summary}{("\nDo Not Get Tired: " + activeMinion.character.needsComponent.doNotGetTired)}";
        summary = $"{summary}{("\nDo Not Get Bored: " + activeMinion.character.needsComponent.doNotGetBored)}";
        summary = $"{summary}{("\nDo Not Recover HP: " + activeMinion.character.doNotRecoverHP)}";
        summary = $"{summary}{("\nCan Move: " + activeMinion.character.canMove)}";
        summary = $"{summary}{("\nCan Witness: " + activeMinion.character.canWitness)}";
        summary = $"{summary}{("\nCan Be Attacked: " + activeMinion.character.canBeAtttacked)}";
        summary = $"{summary}{("\nCan Perform: " + activeMinion.character.canPerform)}";
        summary = $"{summary}{("\nIs Missing: " + activeMinion.character.isMissing)}";
        summary = $"{summary}{("\n" + activeMinion.character.needsComponent.GetNeedsSummary())}";
        summary = $"{summary}{("\nFullness Time: " + (activeMinion.character.needsComponent.fullnessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeMinion.character.needsComponent.fullnessForcedTick)))}";
        summary = $"{summary}{("\nTiredness Time: " + (activeMinion.character.needsComponent.tirednessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeMinion.character.needsComponent.tirednessForcedTick)))}";
        summary = $"{summary}{("\nRemaining Sleep Ticks: " + activeMinion.character.needsComponent.currentSleepTicks)}";
        summary = $"{summary}{("\nFood: " + activeMinion.character.food)}";
        // summary = $"{summary}{("\nRole: " + activeCharacter.role.roleType.ToString())}";
        summary = $"{summary}{("\nSexuality: " + activeMinion.character.sexuality.ToString())}";
        summary = $"{summary}{("\nMood: " + activeMinion.character.moodComponent.moodValue + "/100" + "(" + activeMinion.character.moodComponent.moodState.ToString() + ")")}";
        summary = $"{summary}{("\nHP: " + activeMinion.character.currentHP + "/" + activeMinion.character.maxHP)}";
        summary = $"{summary}{("\nIgnore Hostiles: " + activeMinion.character.ignoreHostility)}";
        summary = $"{summary}{("\nAttack Range: " + activeMinion.character.characterClass.attackRange)}";
        summary = $"{summary}{("\nAttack Speed: " + activeMinion.character.attackSpeed)}";
        summary = $"{summary}{("\nCombat Mode: " + activeMinion.character.combatComponent.combatMode.ToString())}";
        summary = $"{summary}{("\nElemental Type: " + activeMinion.character.combatComponent.elementalDamage.name)}";

        if (activeMinion.character.stateComponent.currentState != null) {
            summary = $"{summary}{$"\nCurrent State: {activeMinion.character.stateComponent.currentState}"}";
            summary = $"{summary}{$"\n\tDuration in state: {activeMinion.character.stateComponent.currentState.currentDuration}/{activeMinion.character.stateComponent.currentState.duration}"}";
        }
        
        summary += "\nBehaviour Components: ";
        for (int i = 0; i < activeMinion.character.behaviourComponent.currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = activeMinion.character.behaviourComponent.currentBehaviourComponents[i];
            summary += $"{component.ToString()}, ";
        }
        
        summary += "\nPersonal Job Queue: ";
        if (activeMinion.character.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < activeMinion.character.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = activeMinion.character.jobQueue.jobsInQueue[i];
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
    }
    public void HideCharacterTestingInfo() {
        UIManager.Instance.HideSmallInfo();
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
        Vector3 actionWorldPos = action.actionItem.transform.localPosition;
        UIManager.Instance.customDropdownList.SetPosition(new Vector3(actionWorldPos.x, actionWorldPos.y + 10f, actionWorldPos.z));
    }
    private bool CanChoostCombatMode(string mode) {
        if(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeMinion.character.combatComponent.combatMode.ToString())
            == mode) {
            return false;
        }
        return true;
    }
    private void OnClickChooseCombatMode(string mode) {
        COMBAT_MODE combatMode = (COMBAT_MODE) System.Enum.Parse(typeof(COMBAT_MODE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(mode));
        UIManager.Instance.characterInfoUI.activeCharacter.combatComponent.SetCombatMode(combatMode);
        Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, activeMinion as IPlayerActionTarget);
        UIManager.Instance.customDropdownList.Close();
    }
    #endregion
}
