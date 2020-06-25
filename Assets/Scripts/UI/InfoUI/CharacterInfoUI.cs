using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inner_Maps;
using TMPro;
using UnityEngine.UI;
using Traits;
using UnityEngine.Serialization;

public class CharacterInfoUI : InfoUIBase {
    
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI plansLbl;
    [SerializeField] private LogItem plansLblLogItem;

    [Space(10)] [Header("Location")]
    [SerializeField] private TextMeshProUGUI factionLbl;
    [SerializeField] private EventLabel factionEventLbl;
    [SerializeField] private TextMeshProUGUI currentLocationLbl;
    [SerializeField] private EventLabel currentLocationEventLbl;
    [SerializeField] private TextMeshProUGUI homeRegionLbl;
    [SerializeField] private EventLabel homeRegionEventLbl;
    [SerializeField] private TextMeshProUGUI houseLbl;
    [SerializeField] private EventLabel houseEventLbl;

    [Space(10)] [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private UIHoverPosition logHoverPosition;
    private LogHistoryItem[] logHistoryItems;

    [Space(10)] [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;
    [SerializeField] private TextMeshProUGUI raceLbl;
    [SerializeField] private TextMeshProUGUI elementLbl;

    [Space(10)] [Header("Traits")]
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)] [Header("Items")]
    [SerializeField] private TextMeshProUGUI itemsLbl;
    
    [Space(10)] [Header("Relationships")]
    [SerializeField] private EventLabel relationshipNamesEventLbl;
    [SerializeField] private TextMeshProUGUI relationshipTypesLbl;
    [SerializeField] private TextMeshProUGUI relationshipNamesLbl;
    [SerializeField] private TextMeshProUGUI relationshipValuesLbl;
    [SerializeField] private UIHoverPosition relationshipNameplateItemPosition;
    
    [Space(10)] [Header("Mood")] 
    [SerializeField] private MarkedMeter moodMeter;
    [SerializeField] private TextMeshProUGUI moodSummary;
    
    [Space(10)] [Header("Needs")] 
    [SerializeField] private MarkedMeter energyMeter;
    [SerializeField] private MarkedMeter fullnessMeter;
    [SerializeField] private MarkedMeter happinessMeter;
    [SerializeField] private MarkedMeter hopeMeter;
    [SerializeField] private MarkedMeter staminaMeter;

    private Character _activeCharacter;
    private Character _previousCharacter;

    public Character activeCharacter => _activeCharacter;
    public Character previousCharacter => _previousCharacter;
    private List<SPELL_TYPE> afflictions;
    private List<string> combatModes;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<IPointOfInterest>(Signals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<Relatable, Relatable>(Signals.RELATIONSHIP_ADDED, OnRelationshipAdded);
        Messenger.AddListener<Character, Character>(Signals.OPINION_ADDED, OnOpinionChanged);
        Messenger.AddListener<Character, Character>(Signals.OPINION_REMOVED, OnOpinionChanged);
        Messenger.AddListener<Character, Character, string>(Signals.OPINION_INCREASED, OnOpinionChanged);
        Messenger.AddListener<Character, Character, string>(Signals.OPINION_DECREASED, OnOpinionChanged);

        Messenger.AddListener<Character>(Signals.UPDATE_THOUGHT_BUBBLE, UpdateThoughtBubbleFromSignal);
        Messenger.AddListener<MoodComponent>(Signals.MOOD_SUMMARY_MODIFIED, OnMoodModified);

        normalTraitsEventLbl.SetOnClickAction(OnClickTrait);
        relationshipNamesEventLbl.SetOnClickAction(OnClickCharacter);
        
        factionEventLbl.SetOnClickAction(OnClickFaction);
        currentLocationEventLbl.SetOnClickAction(OnClickCurrentLocation);
        homeRegionEventLbl.SetOnClickAction(OnClickHomeLocation);
        houseEventLbl.SetOnClickAction(OnClickHomeStructure);

        moodMeter.ResetMarks();
        moodMeter.AddMark(EditableValuesManager.Instance.criticalMoodHighThreshold/100f, Color.red);
        moodMeter.AddMark(EditableValuesManager.Instance.lowMoodHighThreshold/100f, Color.yellow);

        energyMeter.ResetMarks();
        energyMeter.AddMark(CharacterNeedsComponent.REFRESHED_LOWER_LIMIT/100f, Color.green);
        energyMeter.AddMark(CharacterNeedsComponent.TIRED_UPPER_LIMIT/100f, Color.yellow);
        energyMeter.AddMark(CharacterNeedsComponent.EXHAUSTED_UPPER_LIMIT/100f, Color.red);
        
        fullnessMeter.ResetMarks();
        fullnessMeter.AddMark(CharacterNeedsComponent.FULL_LOWER_LIMIT/100f, Color.green);
        fullnessMeter.AddMark(CharacterNeedsComponent.HUNGRY_UPPER_LIMIT/100f, Color.yellow);
        fullnessMeter.AddMark(CharacterNeedsComponent.STARVING_UPPER_LIMIT/100f, Color.red);
        
        happinessMeter.ResetMarks();
        happinessMeter.AddMark(CharacterNeedsComponent.ENTERTAINED_LOWER_LIMIT/100f, Color.green);
        happinessMeter.AddMark(CharacterNeedsComponent.BORED_UPPER_LIMIT/100f, Color.yellow);
        happinessMeter.AddMark(CharacterNeedsComponent.SULKING_UPPER_LIMIT/100f, Color.red);
        
        staminaMeter.ResetMarks();
        staminaMeter.AddMark(CharacterNeedsComponent.SPRIGHTLY_LOWER_LIMIT/100f, Color.green);
        staminaMeter.AddMark(CharacterNeedsComponent.SPENT_UPPER_LIMIT/100f, Color.yellow);
        staminaMeter.AddMark(CharacterNeedsComponent.DRAINED_UPPER_LIMIT/100f, Color.red);
        
        hopeMeter.ResetMarks();
        hopeMeter.AddMark(CharacterNeedsComponent.HOPEFUL_LOWER_LIMIT/100f, Color.green);
        hopeMeter.AddMark(CharacterNeedsComponent.DISCOURAGED_UPPER_LIMIT/100f, Color.yellow);
        hopeMeter.AddMark(CharacterNeedsComponent.HOPELESS_UPPER_LIMIT/100f, Color.red);
        
        InitializeLogsMenu();

        afflictions = new List<SPELL_TYPE>();
        ConstructCombatModes();
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        if (_activeCharacter != null && ReferenceEquals(_activeCharacter.marker, null) == false) {
            if (InnerMapCameraMove.Instance.target == _activeCharacter.marker.gameObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
            _activeCharacter.marker.HideThoughts();
        }
        _activeCharacter = null;
    }
    public override void OpenMenu() {
        _previousCharacter = _activeCharacter;
        _activeCharacter = _data as Character;
        base.OpenMenu();
        if (_previousCharacter != null && _previousCharacter.marker != null) {
            _previousCharacter.marker.HideThoughts();
        }
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        if (_activeCharacter.marker && _activeCharacter.marker.transform != null) {
            Selector.Instance.Select(_activeCharacter, _activeCharacter.marker.transform);
            _activeCharacter.marker.ShowThoughts();
        }
        UpdateCharacterInfo();
        UpdateTraits();
        UpdateRelationships();
        UpdateInventoryInfo();
        UpdateHistory(_activeCharacter);
        ResetAllScrollPositions();
        UpdateMoodSummary();
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
    public void UpdateCharacterInfo() {
        if (_activeCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
        UpdateLocationInfo();
        UpdateMoodMeter();
        UpdateNeedMeters();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeCharacter.visuals.GetNameplateName();
        lvlClassLbl.text = _activeCharacter.raceClassName;
        UpdateThoughtBubble();
    }
    public void UpdateThoughtBubble() {
        plansLbl.text = activeCharacter.visuals.GetThoughtBubble(out var log);
        if (log != null) {
            plansLblLogItem.SetLog(log);
        }
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = $"{_activeCharacter.currentHP.ToString()}/{_activeCharacter.maxHP.ToString()}";
        attackLbl.text = $"{_activeCharacter.combatComponent.attack.ToString()}";
        speedLbl.text =  $"{_activeCharacter.combatComponent.attackSpeed / 1000f}s";
        raceLbl.text = $"{UtilityScripts.GameUtilities.GetNormalizedSingularRace(_activeCharacter.race)}";
        elementLbl.text = $"{_activeCharacter.combatComponent.elementalDamage.type.ToString()}";
        //if(characterPortrait.character != null) {
        //    characterPortrait.UpdateLvl();
        //}
    }
    #endregion

    #region Location
    private void UpdateLocationInfo() {
        factionLbl.text = _activeCharacter.faction != null ? $"<link=\"faction\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.faction.name)}</link>" : "Factionless";
        currentLocationLbl.text = $"{_activeCharacter.currentRegion.name}";
        homeRegionLbl.text = _activeCharacter.homeRegion != null ? $"{_activeCharacter.homeRegion.name}" : "Homeless";
        //currentLocationLbl.text = $"<link=\"currLocation\">{UtilityScripts.Utilities.ColorizeName(_activeCharacter.currentRegion.name)}</link>";
        //homeRegionLbl.text = _activeCharacter.homeRegion != null ? $"<link=\"home\">{UtilityScripts.Utilities.ColorizeName(_activeCharacter.homeRegion.name)}</link>" : "Homeless";
        houseLbl.text = _activeCharacter.homeStructure != null ? $"<link=\"house\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.homeStructure.name)}</link>" : "Homeless";
    }
    private void OnClickFaction(object obj) {
        UIManager.Instance.ShowFactionInfo(activeCharacter.faction);
    }
    private void OnClickCurrentLocation(object obj) {
        UIManager.Instance.ShowRegionInfo(activeCharacter.currentRegion);
    }
    private void OnClickHomeLocation(object obj) {
        UIManager.Instance.ShowRegionInfo(activeCharacter.homeRegion);
    }
    private void OnClickHomeStructure(object obj) {
        if (activeCharacter.homeRegion != null) {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != activeCharacter.homeRegion.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (activeCharacter.homeRegion.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(activeCharacter.homeRegion);
            }
            InnerMapCameraMove.Instance.CenterCameraOn(activeCharacter.homeStructure.structureObj.gameObject);
        }
        
    }
    #endregion

    #region Traits
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
    }
    private void UpdateThoughtBubbleFromSignal(Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateThoughtBubble();
        }
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;

        for (int i = 0; i < _activeCharacter.traitContainer.statuses.Count; i++) {
            Status currStatus = _activeCharacter.traitContainer.statuses[i];
            if (currStatus.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (currStatus.moodEffect > 0) {
                color = UIManager.buffTextColor;
            } else if (currStatus.moodEffect < 0) {
                color = UIManager.flawTextColor;
            }
            if (!string.IsNullOrEmpty(statusTraits)) {
                statusTraits = $"{statusTraits}, ";
            }
            statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currStatus.GetNameInUI(activeCharacter)}</link></color></b>";
        }
        for (int i = 0; i < _activeCharacter.traitContainer.traits.Count; i++) {
            Trait currTrait = _activeCharacter.traitContainer.traits[i];
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
            normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(activeCharacter)}</link></color></b>";
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
            Trait trait = activeCharacter.traitContainer.traits[index];
            UIManager.Instance.ShowSmallInfo(trait.description);
        }
    }
    public void OnHoverStatus(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeCharacter.traitContainer.statuses[index];
            UIManager.Instance.ShowSmallInfo(trait.description);
        }
    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnClickTrait(object obj) {
#if UNITY_EDITOR
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeCharacter.traitContainer.traits[index];
            string traitDescription = trait.description;
            if (trait.canBeTriggered) {
                traitDescription +=
                    $"\n{trait.GetRequirementDescription(activeCharacter)}\n\n<b>Effect</b>: {trait.GetTriggerFlawEffectDescription(activeCharacter, "flaw_effect")}";
            }

            StartCoroutine(HoverOutTraitAfterClick());//Quick fix because tooltips do not disappear. Issue with hover out action in label not being called when other collider goes over it.
            UIManager.Instance.ShowYesNoConfirmation(trait.name, traitDescription,
                onClickYesAction: () => OnClickTriggerFlaw(trait),
                onClickNoAction: () => OnClickRemoveTrait(trait),
                showCover: true, layer: 25,
                yesBtnText: $"Trigger ({EditableValuesManager.Instance.triggerFlawManaCost.ToString()} Mana)",
                noBtnText: "Remove Trait",
                yesBtnInteractable: PlayerManager.Instance.player.playerSkillComponent.canTriggerFlaw && trait.canBeTriggered && trait.CanFlawBeTriggered(activeCharacter) && TraitManager.Instance.CanStillTriggerFlaws(activeCharacter),
                noBtnInteractable: PlayerManager.Instance.player.playerSkillComponent.canRemoveTraits,
                pauseAndResume: true,
                //noBtnActive: false,
                //yesBtnActive: trait.canBeTriggered,
                yesBtnInactiveHoverAction: () => ShowCannotTriggerFlawReason(trait),
                yesBtnInactiveHoverExitAction: UIManager.Instance.HideSmallInfo
            );
            normalTraitsEventLbl.ResetHighlightValues();
            if (trait.type == TRAIT_TYPE.FLAW) {
                Messenger.Broadcast(Signals.FLAW_CLICKED, trait);
            }
        }
#endif
    }
    private IEnumerator HoverOutTraitAfterClick() {
        yield return new WaitForEndOfFrame();
        OnHoverOutTrait();
    }
    private void ShowCannotTriggerFlawReason(Trait trait) {
        string reason = $"You cannot trigger {activeCharacter.name}'s flaw because: ";
        List<string> reasons = trait.GetCannotTriggerFlawReasons(activeCharacter);
        for (int i = 0; i < reasons.Count; i++) {
            reason = $"{reason}\n\t- {reasons[i]}";
        }
        UIManager.Instance.ShowSmallInfo(reason);
    }
    private void OnClickTriggerFlaw(Trait trait) {
        string logKey = trait.TriggerFlaw(activeCharacter);
        int manaCost = EditableValuesManager.Instance.triggerFlawManaCost;
        PlayerManager.Instance.player.AdjustMana(-manaCost);
        if (logKey != "flaw_effect") {
            UIManager.Instance.ShowYesNoConfirmation(
                trait.name,
                trait.GetTriggerFlawEffectDescription(activeCharacter, logKey),
                showCover: true,
                layer: 25,
                yesBtnText: "OK",
                pauseAndResume: true,
                noBtnActive: false
            );
        }
        Messenger.Broadcast(Signals.FLAW_TRIGGERED_BY_PLAYER);
    }
    private void OnClickRemoveTrait(Trait trait) {
        activeCharacter.traitContainer.RemoveTrait(activeCharacter, trait);
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(TileObject item, Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        itemsLbl.text = string.Empty;
        for (int i = 0; i < _activeCharacter.items.Count; i++) {
            TileObject currInventoryItem = _activeCharacter.items[i];
            itemsLbl.text = $"{itemsLbl.text}{currInventoryItem.name}";
            if (i < _activeCharacter.items.Count - 1) {
                itemsLbl.text = $"{itemsLbl.text}, ";
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(IPointOfInterest poi) {
        if (isShowing && poi == _activeCharacter) {
            UpdateAllHistoryInfo();
            //if (_activeCharacter.minion != null) {
            //    ClearHistory();
            //} else if (poi != null && _activeCharacter != null && poi == _activeCharacter) {
            //    UpdateAllHistoryInfo();
            //}    
        }
    }
    private void UpdateAllHistoryInfo() {
        //if (_activeCharacter.minion != null) {
        //    return;
        //}
        //List<Log> characterHistory = new List<Log>(_activeCharacter.history.OrderByDescending(x => x.date.year).ThenByDescending(x => x.date.month).ThenByDescending(x => x.date.day).ThenByDescending(x => x.date.tick));
        int historyCount = _activeCharacter.logComponent.history.Count;
        int historyLastIndex = historyCount - 1;
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < historyCount) {
                Log currLog = _activeCharacter.logComponent.history[historyLastIndex - i];
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
    private void OnMenuOpened(InfoUIBase openedBase) {
        //if (this.isShowing) {
        //    if (openedMenu is PartyInfoUI) {
        //        CheckIfMenuShouldBeHidden();
        //    }
        //}
    }
    private void OnMenuClosed(InfoUIBase closedBase) {
        //if (this.isShowing) {
        //    if (closedMenu is PartyInfoUI) {
        //        CheckIfMenuShouldBeHidden();
        //    }
        //}
    }
    private void OnOpenShareIntelMenu() {
        backButton.interactable = false;
    }
    //private void OnCloseShareIntelMenu() { }
    //private void OnCharacterChangedAlterEgo(Character character) {
    //    if (isShowing && activeCharacter == character) {
    //        UpdateCharacterInfo();
    //        UpdateTraits();
    //    }
    //}
    private void OnCharacterDied(Character character) {
        if (this.isShowing && activeCharacter.id == character.id) {
            InnerMapCameraMove.Instance.CenterCameraOn(null);
        }
    }
    #endregion

    #region For Testing
    public void ShowCharacterTestingInfo() {
#if UNITY_EDITOR
        string summary = $"Home structure: {activeCharacter.homeStructure?.ToString() ?? "None"}" ?? "None";
        summary = $"{summary}{($"\nCurrent structure: {activeCharacter.currentStructure}" ?? "None")}";
        summary = $"{summary}{("\nPOI State: " + activeCharacter.state.ToString())}";
        summary = $"{summary}{("\nDo Not Get Hungry: " + activeCharacter.needsComponent.doNotGetHungry.ToString())}";
        summary = $"{summary}{("\nDo Not Get Tired: " + activeCharacter.needsComponent.doNotGetTired.ToString())}";
        summary = $"{summary}{("\nDo Not Get Bored: " + activeCharacter.needsComponent.doNotGetBored.ToString())}";
        summary = $"{summary}{("\nDo Not Recover HP: " + activeCharacter.doNotRecoverHP.ToString())}";
        summary = $"{summary}{("\nCan Move: " + activeCharacter.canMove)}";
        summary = $"{summary}{("\nCan Witness: " + activeCharacter.canWitness)}";
        summary = $"{summary}{("\nCan Be Attacked: " + activeCharacter.canBeAttacked)}";
        summary = $"{summary}{("\nCan Perform: " + activeCharacter.canPerform)}";
        summary = $"{summary}{("\nIs Missing: " + activeCharacter.isMissing)}";
        summary = $"{summary}{("\nIs Running: " + activeCharacter.movementComponent.isRunning)}";
        summary = $"{summary}{("\nPOI State: " + activeCharacter.state.ToString())}";
        summary = $"{summary}{("\n" + activeCharacter.needsComponent.GetNeedsSummary())}";
        summary = $"{summary}{("\nFullness Time: " + (activeCharacter.needsComponent.fullnessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.fullnessForcedTick)))}";
        summary = $"{summary}{("\nTiredness Time: " + (activeCharacter.needsComponent.tirednessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.tirednessForcedTick)))}";
        summary = $"{summary}{("\nRemaining Sleep Ticks: " + activeCharacter.needsComponent.currentSleepTicks.ToString())}";
        summary = $"{summary}{("\nFood: " + activeCharacter.food.ToString())}";
        // summary = $"{summary}{("\nRole: " + activeCharacter.role.roleType.ToString())}";
        summary = $"{summary}{("\nSexuality: " + activeCharacter.sexuality.ToString())}";
        summary = $"{summary}{("\nMood: " + activeCharacter.moodComponent.moodValue + "/100" + "(" + activeCharacter.moodComponent.moodState.ToString() + ")")}";
        summary = $"{summary}{("\nHP: " + activeCharacter.currentHP.ToString() + "/" + activeCharacter.maxHP.ToString())}";
        summary = $"{summary}{("\nAttack Range: " + activeCharacter.characterClass.attackRange.ToString(CultureInfo.InvariantCulture))}";
        summary = $"{summary}{("\nAttack Speed: " + activeCharacter.combatComponent.attackSpeed.ToString())}";
        summary = $"{summary}{("\nCombat Mode: " + activeCharacter.combatComponent.combatMode.ToString())}";
        summary = $"{summary}{("\nElemental Type: " + activeCharacter.combatComponent.elementalDamage.name)}";
        summary = $"{summary}{("\nPrimary Job: " + activeCharacter.jobComponent.primaryJob.ToString())}";
        summary = $"{summary}{("\nPriority Jobs: " + activeCharacter.jobComponent.GetPriorityJobs())}";
        summary = $"{summary}{("\nSecondary Jobs: " + activeCharacter.jobComponent.GetSecondaryJobs())}";
        summary = $"{summary}{("\nAble Jobs: " + activeCharacter.jobComponent.GetAbleJobs())}";

        if (activeCharacter.stateComponent.currentState != null) {
            summary = $"{summary}\nCurrent State: {activeCharacter.stateComponent.currentState}";
            summary = $"{summary}\n\tDuration in state: {activeCharacter.stateComponent.currentState.currentDuration.ToString()}/{activeCharacter.stateComponent.currentState.duration.ToString()}";
        }
        
        summary += "\nBehaviour Components: ";
        for (int i = 0; i < activeCharacter.behaviourComponent.currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = activeCharacter.behaviourComponent.currentBehaviourComponents[i];
            summary += $"{component.ToString()}, ";
        }
        
        summary += "\nInterested Items: ";
        for (int i = 0; i < activeCharacter.interestedItemNames.Count; i++) {
            summary += $"{activeCharacter.interestedItemNames[i]}, ";
        }
        
        summary += "\nPersonal Job Queue: ";
        if (activeCharacter.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < activeCharacter.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = activeCharacter.jobQueue.jobsInQueue[i];
                summary += $"{poi}, ";
            }
        } else {
            summary += "None";
        }
        // summary += "\n" + activeCharacter.needsComponent.GetNeedsSummary();
        UIManager.Instance.ShowSmallInfo(summary);
#endif
    }
    public void HideCharacterTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Relationships
    private void UpdateRelationships() {
        relationshipTypesLbl.text = string.Empty;
        relationshipNamesLbl.text = string.Empty;
        relationshipValuesLbl.text = string.Empty;
        Dictionary<int, IRelationshipData> orderedRels = _activeCharacter.relationshipContainer.relationships
            .OrderByDescending(k => k.Value.opinions.totalOpinion)
            .ToDictionary(k => k.Key, v => v.Value);

        List<int> keys = _activeCharacter.relationshipContainer.relationships.Keys.ToList();
        for (int i = 0; i < orderedRels.Keys.Count; i++) {
            int targetID = orderedRels.Keys.ElementAt(i);
            int actualIndex = keys.IndexOf(targetID);
            IRelationshipData relationshipData =
                _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
            relationshipTypesLbl.text += $"{_activeCharacter.relationshipContainer.GetRelationshipNameWith(targetID)}\n";
            
            int opinionOfOther = 0;
            string opinionText;
            Character target = CharacterManager.Instance.GetCharacterByID(targetID);
            if (target != null && target.relationshipContainer.HasRelationshipWith(activeCharacter)) {
                opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
                opinionText = GetOpinionText(opinionOfOther);
            } else {
                opinionText = "???";
            }
            
            relationshipNamesLbl.text += $"<link=\"{actualIndex.ToString()}\">{UtilityScripts.Utilities.ColorizeAndBoldName(relationshipData.targetName)}</link>\n";
            relationshipValuesLbl.text +=
                $"<link=\"{actualIndex.ToString()}\"><color={BaseRelationshipContainer.OpinionColor(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}> {GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color> <color={BaseRelationshipContainer.OpinionColor(opinionOfOther)}>({opinionText})</color></link>\n";
        }
    }
    public void OnHoverRelationshipValue(object obj) {
        if (obj is string) {
            string text = (string)obj;
            int index = int.Parse(text);
            int id = _activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index);
            ShowOpinionData(id);
        }
    }
    public void OnHoverRelationshipName(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            int characterID = _activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index);
            OnHoverCharacterNameInRelationships(characterID);
        }
    }
    private void OnOpinionChanged(Character owner, Character target, string reason) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void OnOpinionChanged(Character owner, Character target) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void OnRelationshipAdded(Relatable owner, Relatable target) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void ShowOpinionData(int targetID) {
        IRelationshipData targetData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
        Character target = CharacterManager.Instance.GetCharacterByID(targetID);

        string summary = $"{activeCharacter.name}'s opinion of {targetData.targetName}";
        summary += "\n---------------------";
        Dictionary<string, int> opinions = activeCharacter.relationshipContainer.GetOpinionData(targetID).allOpinions;
        foreach (KeyValuePair<string, int> kvp in opinions) {
            summary += $"\n{kvp.Key}: <color={BaseRelationshipContainer.OpinionColor(kvp.Value)}>{GetOpinionText(kvp.Value)}</color>";
        }
        summary += "\n---------------------";
        summary +=
            $"\nTotal: <color={BaseRelationshipContainer.OpinionColor(targetData.opinions.totalOpinion)}>{GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color>";
        if (target != null) {
            int opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
            summary +=
                $"\n{targetData.targetName}'s opinion of {activeCharacter.name}: <color={BaseRelationshipContainer.OpinionColor(opinionOfOther)}>{GetOpinionText(opinionOfOther)}</color>";
        } else {
            summary += $"\n{targetData.targetName}'s opinion of {activeCharacter.name}: ???</color>";
        }
        
        summary +=
            $"\n\nCompatibility: {RelationshipManager.Instance.GetCompatibilityBetween(activeCharacter, targetID).ToString()}";
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideRelationshipData() {
        UIManager.Instance.HideSmallInfo();
    }
    private string GetOpinionText(int number) {
        if (number < 0) {
            return $"{number.ToString()}";
        }
        return $"+{number.ToString()}";
    }
    private void OnClickCharacter(object obj) {
        if (obj is string) {
            string text = (string)obj;
            int index = int.Parse(text);
            Character target = CharacterManager.Instance.GetCharacterByID(_activeCharacter.relationshipContainer
                .relationships.Keys.ElementAtOrDefault(index));
            if (target != null) {
                UIManager.Instance.ShowCharacterInfo(target,true);    
            }
        }
    }
    private void OnHoverCharacterNameInRelationships(int id) {
        Character target = CharacterManager.Instance.GetCharacterByID(id);
        if (target != null) {
            UIManager.Instance.HideSmallInfo();
            UIManager.Instance.ShowCharacterNameplateTooltip(target, relationshipNameplateItemPosition);
        } else {
            //character has not yet been spawned
            IRelationshipData relationshipData = _activeCharacter.relationshipContainer.relationships[id];
            UIManager.Instance.ShowSmallInfo($"{relationshipData.targetName} is not yet in this region.", relationshipNameplateItemPosition);
            UIManager.Instance.HideCharacterNameplateTooltip();
        }
    }
    public void HideRelationshipNameplate() {
        UIManager.Instance.HideSmallInfo();
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
    #endregion

    #region Afflict
    public void ShowAfflictUI() {
        afflictions.Clear();
        List<SPELL_TYPE> afflictionTypes = PlayerManager.Instance.player.playerSkillComponent.afflictions;
        for (int i = 0; i < afflictionTypes.Count; i++) {
            SPELL_TYPE spellType = afflictionTypes[i];
            afflictions.Add(spellType);
        }
        //foreach (SpellData abilityData in PlayerManager.Instance.allSpellsData.Values) {
        //    if (abilityData.type == INTERVENTION_ABILITY_TYPE.AFFLICTION) {
        //        afflictions.Add(abilityData.name);
        //    }
        //}
        UIManager.Instance.ShowClickableObjectPicker(afflictions, ActivateAfflictionConfirmation, null, CanActivateAffliction,
            "Select Affliction", OnHoverAffliction, OnHoverOutAffliction, portraitGetter: GetAfflictionPortrait, identifier: "Intervention Ability", showCover: true, layer: 19, shouldShowConfirmationWindowOnPick: true, asButton: true);
    }
    private Sprite GetAfflictionPortrait(string str) {
        return PlayerManager.Instance.GetJobActionSprite(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(str));
    }
    private void ActivateAfflictionConfirmation(object o) {
        SPELL_TYPE afflictionType = (SPELL_TYPE)o;
        string afflictionName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(afflictionType.ToString());
        UIManager.Instance.ShowYesNoConfirmation("Affliction Confirmation", "Are you sure you want to afflict " + afflictionName + "?", () => ActivateAffliction(afflictionType));
    }
    private void ActivateAffliction(SPELL_TYPE afflictionType) {
        UIManager.Instance.HideObjectPicker();
        PlayerSkillManager.Instance.GetAfflictionData(afflictionType).ActivateAbility(activeCharacter);
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.AFFLICT).OnExecuteSpellActionAffliction();
    }
    private bool CanActivateAffliction(SPELL_TYPE afflictionType) {
        if (WorldConfigManager.Instance.isDemoWorld) {
            return WorldConfigManager.Instance.availableSpellsInDemoBuild.Contains(afflictionType) 
                   && PlayerSkillManager.Instance.GetAfflictionData(afflictionType).CanPerformAbilityTowards(activeCharacter);
        }
        return PlayerSkillManager.Instance.GetAfflictionData(afflictionType).CanPerformAbilityTowards(activeCharacter);
    }
    private void OnHoverAffliction(SPELL_TYPE afflictionType) {
        SpellData affliction = PlayerSkillManager.Instance.GetAfflictionData(afflictionType);
        //UIManager.Instance.ShowSmallInfo(affliction.description + "\n" + affliction.GetManaCostChargesCooldownStr());
        PlayerUI.Instance.OnHoverSpell(affliction);
    }
    private void OnHoverOutAffliction(SPELL_TYPE afflictionType) {
        UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.OnHoverOutSpell(null);
    }
    #endregion

    #region Mood
    private void OnMoodModified(MoodComponent moodComponent) {
        if (_activeCharacter != null && _activeCharacter.moodComponent == moodComponent) {
            UpdateMoodMeter();
            UpdateMoodSummary();
        }
    }
    private void UpdateMoodMeter() {
        moodMeter.SetFillAmount(_activeCharacter.moodComponent.moodValue/100f);
    }
    private void UpdateMoodSummary() {
        moodSummary.text = string.Empty;
        string summary = string.Empty;
        foreach (KeyValuePair<string, int> pair in _activeCharacter.moodComponent.moodModificationsSummary) {
            string color = "green";
            string text = "+" + pair.Value;
            if (pair.Value < 0) {
                color = "red";
                text = pair.Value.ToString();
            }
            summary += $"<color={color}>{text}</color> {pair.Key}\n";
        }
        moodSummary.text = summary;
    }
    public void ShowMoodTooltip() {
        string summary = $"Represents the Villager's overall state of mind. Lower a Villager's Mood to make him less effective and more volatile.\n\n" +
                         $"{_activeCharacter.moodComponent.moodValue.ToString()}/100";
        // summary +=
        //     $"\nChance to trigger Major Mental Break {_activeCharacter.moodComponent.currentCriticalMoodEffectChance.ToString(CultureInfo.InvariantCulture)}";
        UIManager.Instance.ShowSmallInfo(summary, "MOOD");
    }
    public void HideSmallInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Needs
    private void UpdateNeedMeters() {
        energyMeter.SetFillAmount(_activeCharacter.needsComponent.tiredness/CharacterNeedsComponent.TIREDNESS_DEFAULT);
        fullnessMeter.SetFillAmount(_activeCharacter.needsComponent.fullness/CharacterNeedsComponent.FULLNESS_DEFAULT);
        happinessMeter.SetFillAmount(_activeCharacter.needsComponent.happiness/CharacterNeedsComponent.HAPPINESS_DEFAULT);
        hopeMeter.SetFillAmount(_activeCharacter.needsComponent.hope/CharacterNeedsComponent.HOPE_DEFAULT);
        staminaMeter.SetFillAmount(_activeCharacter.needsComponent.stamina/CharacterNeedsComponent.STAMINA_DEFAULT);
    }
    public void ShowEnergyTooltip() {
        string summary = $"Villagers will become Unconscious once this Meter is empty. This is replenished through rest.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.tiredness.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "ENERGY");
    }
    public void ShowFullnessTooltip() {
        string summary = $"Villagers will become Malnourished and eventually die once this Meter is empty. This is replenished through eating.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.fullness.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "FULLNESS");
    }
    public void ShowHappinessTooltip() {
        string summary = $"Villager's Mood becomes significantly affected when this Meter goes down. This is replenished by doing fun activities.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.happiness.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "ENTERTAINMENT");
    }
    public void ShowHopeTooltip() {
        string summary = $"For future implementation.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.hope.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "HOPE");
    }
    public void ShowStaminaTooltip() {
        string summary = $"Villagers will be unable to run when this Meter is empty. This is used up when the Villager is running and quickly replenished when he isn't.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.stamina.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "STAMINA");
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
        if(actionItem != null) {
            Vector3 actionWorldPos = actionItem.transform.localPosition;
            UIManager.Instance.customDropdownList.SetPosition(new Vector3(actionWorldPos.x, actionWorldPos.y + 10f, actionWorldPos.z));
        }
    }
    private bool CanChoostCombatMode(string mode) {
        if(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeCharacter.combatComponent.combatMode.ToString()) == mode) {
            return false;
        }
        return true;
    }
    private void OnClickChooseCombatMode(string mode) {
        COMBAT_MODE combatMode = (COMBAT_MODE) System.Enum.Parse(typeof(COMBAT_MODE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(mode));
        UIManager.Instance.characterInfoUI.activeCharacter.combatComponent.SetCombatMode(combatMode);
        Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, activeCharacter as IPlayerActionTarget);
        UIManager.Instance.customDropdownList.Close();
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.CHANGE_COMBAT_MODE).OnExecuteSpellActionAffliction();
    }
    #endregion

    #region Tabs
    public void OnToggleInfo(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Info");    
        // }
    }
    public void OnToggleMood(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Mood");    
        // }
    }
    public void OnToggleRelations(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Relations");    
        // }
    }
    public void OnToggleLogs(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Logs");    
        // }
    }
    #endregion
}
