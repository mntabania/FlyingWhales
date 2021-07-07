using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using TMPro;
using UnityEngine.UI;
using Traits;
using UtilityScripts;
using Inner_Maps;
using Ruinarch.Custom_UI;
using Character_Talents;
public class CharacterInfoUI : InfoUIBase {

    private enum VIEW_MODE { None = 0, Info, Mood, Relationship, Logs, }
    private VIEW_MODE m_currentViewMode = VIEW_MODE.None;
    
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private TextMeshProUGUI actionLbl;
    [SerializeField] private EventLabel actionEventLabel;
    [SerializeField] private TextMeshProUGUI partyLbl;
    [SerializeField] private EventLabel partyEventLbl;
    [SerializeField] private Image raceIcon;
    [SerializeField] private TextMeshProUGUI coinsLbl;

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
    [SerializeField] private LogsWindow _logsWindow;

    [Space(10)] [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;
    [SerializeField] private TextMeshProUGUI titlePiercingLbl;
    [SerializeField] private TextMeshProUGUI piercingLbl;
    [SerializeField] private TextMeshProUGUI raceLbl;
    [SerializeField] private TextMeshProUGUI elementLbl;
    [SerializeField] private TextMeshProUGUI intLbl;
    [SerializeField] private TextMeshProUGUI critRateLbl;

    [Space(10)] [Header("Traits")]
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)] [Header("Items")]
    [SerializeField] private TextMeshProUGUI itemsLbl;
    [SerializeField] private EventLabel itemsEventLbl;

    [Space(10)]
    [Header("Equips")]
    [SerializeField] private Image weaponImg;
    [SerializeField] private Image armorImg;
    [SerializeField] private Image accessoryImg;
    [SerializeField] private HoverHandler weaponHoverText;
    [SerializeField] private HoverHandler armorHoverText;
    [SerializeField] private HoverHandler accessoryHoverText;
    [SerializeField] private EventEquipButton weaponEventButton;
    [SerializeField] private EventEquipButton armorEventButton;
    [SerializeField] private EventEquipButton accessoryEventButton;
    [SerializeField] private EquipmentToolTip equipmentToolTip;

    [Space(10)]
    [Header("Talents")]
    [SerializeField] private GameObject talentsParentObject;
    [SerializeField] private TextMeshProUGUI martialArtsLbl;
    [SerializeField] private TextMeshProUGUI combatMagicLbl;
    [SerializeField] private TextMeshProUGUI healingMagicLbl;
    [SerializeField] private TextMeshProUGUI craftingLbl;
    [SerializeField] private TextMeshProUGUI resourcesLbl;
    [SerializeField] private TextMeshProUGUI foodLbl;
    [SerializeField] private TextMeshProUGUI socialLbl;
    [SerializeField] private HoverHandler martialArtsHoverText;
    [SerializeField] private HoverHandler combatMagicHoverText;
    [SerializeField] private HoverHandler healingMagicHoverText;
    [SerializeField] private HoverHandler craftingHoverText;
    [SerializeField] private HoverHandler resourcesHoverText;
    [SerializeField] private HoverHandler foodHoverText;
    [SerializeField] private HoverHandler socialHoverText;
    [SerializeField] private CharacterTalentTooltip talentToolTip;

    [Space(10)] [Header("Relationships")]
    [SerializeField] private EventLabel relationshipNamesEventLbl;
    [SerializeField] private TextMeshProUGUI relationshipTypesLbl;
    [SerializeField] private TextMeshProUGUI relationshipNamesLbl;
    [SerializeField] private TextMeshProUGUI relationshipValuesLbl;
    [SerializeField] private UIHoverPosition relationshipNameplateItemPosition;
    [SerializeField] private RelationshipFilterItem[] relationFilterItems;
    [SerializeField] private GameObject relationFiltersGO;
    [SerializeField] private Toggle allRelationshipFiltersToggle;
    [SerializeField] private EventLabel opinionsEventLabel;
    [SerializeField] private ScrollRect relationshipsScrollView;
    
    [Space(10)] [Header("Mood")] 
    [SerializeField] private MarkedMeter moodMeter;
    [SerializeField] private TextMeshProUGUI moodSummary;
    [SerializeField] private ScrollRect scrollViewMoodSummary;
    [SerializeField] private GameObject prefabMoodThought;
    
    [Space(10)] [Header("Needs")] 
    [SerializeField] private MarkedMeter energyMeter;
    [SerializeField] private MarkedMeter fullnessMeter;
    [SerializeField] private MarkedMeter happinessMeter;
    [SerializeField] private MarkedMeter hopeMeter;
    [SerializeField] private MarkedMeter staminaMeter;

    [Space(10)]
    [Header("Piercing And Resistances")]
    [SerializeField] private PiercingAndResistancesInfo piercingAndResistancesInfo;

    [Space(10)]
    [Header("Store Target")] 
    [SerializeField] private StoreTargetButton btnStoreTarget;

    private Character _activeCharacter;
    private Character _previousCharacter;

    public Character activeCharacter => _activeCharacter;
    public Character previousCharacter => _previousCharacter;
    private List<SkillData> afflictions;
    private bool aliveRelationsOnly;
    private List<RELATIONS_FILTER> filters;
    private RELATIONS_FILTER[] allFilters;
    private Dictionary<string, MoodSummaryEntry> _dictMoodSummary;

    #region reveal info objects
    public GameObject infoContent;
    public GameObject btnRevealInfo;
    public GameObject moodContent;
    public GameObject btnRevealMood;
    public GameObject relationshipContent;
    public GameObject btnRevealRelationship;
    public GameObject logContent;
    public GameObject btnRevealLogs;
    #endregion

    #region pierce display UI
    public PiercingAndResistancesInfo pierceUI;
	#endregion

	internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
        Messenger.AddListener<Character>(UISignals.LOG_MENTIONING_CHARACTER_UPDATED, OnLogMentioningCharacterUpdated);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
        Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<TileObject, Character>(CharacterSignals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<TileObject, Character>(CharacterSignals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<Relatable, Relatable>(CharacterSignals.RELATIONSHIP_CREATED, OnRelationshipChanged);
        Messenger.AddListener<Relatable, Relatable>(CharacterSignals.RELATIONSHIP_TYPE_ADDED, OnRelationshipChanged);
        Messenger.AddListener<Character, Character>(CharacterSignals.OPINION_ADDED, OnOpinionChanged);
        Messenger.AddListener<Character, Character>(CharacterSignals.OPINION_REMOVED, OnOpinionChanged);
        Messenger.AddListener<Character, Character, string>(CharacterSignals.OPINION_INCREASED, OnOpinionChanged);
        Messenger.AddListener<Character, Character, string>(CharacterSignals.OPINION_DECREASED, OnOpinionChanged);
        Messenger.AddListener<Character>(UISignals.UPDATE_THOUGHT_BUBBLE, UpdateThoughtBubbleFromSignal);
        Messenger.AddListener<MoodComponent>(CharacterSignals.MOOD_SUMMARY_MODIFIED, OnMoodModified);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
        Messenger.AddListener<Character>(UISignals.UPDATE_CHARACTER_INFO, CharacterRequestedForUpdate);
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
        Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.WEAPON_UNEQUIPPED, OnEquipmentUnequipped);
        Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.ARMOR_UNEQUIPPED, OnEquipmentUnequipped);
        Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.ACCESSORY_UNEQUIPPED, OnEquipmentUnequipped);
        Messenger.AddListener<int, int>(PlayerSignals.PLAGUE_POINTS_ADJUSTED, OnPlaguePointsAdjusted);

        actionEventLabel.SetOnRightClickAction(OnRightClickThoughtBubble);
        relationshipNamesEventLbl.SetOnLeftClickAction(OnLeftClickRelationship);
        relationshipNamesEventLbl.SetOnRightClickAction(OnRightClickRelationship);
        
        factionEventLbl.SetOnLeftClickAction(OnClickFaction);
        homeRegionEventLbl.SetOnLeftClickAction(OnLeftClickHomeVillage);
        homeRegionEventLbl.SetOnRightClickAction(OnRightClickHomeVillage);
        houseEventLbl.SetOnLeftClickAction(OnLeftClickHomeStructure);
        houseEventLbl.SetOnRightClickAction(OnRightClickHomeStructure);
        partyEventLbl.SetOnLeftClickAction(OnClickParty);
        
        itemsEventLbl.SetOnLeftClickAction(OnLeftClickItem);
        itemsEventLbl.SetOnRightClickAction(OnRightClickItem);

        weaponEventButton.AddPointerRightClickAction(OnRightClickEquipment);
        armorEventButton.AddPointerRightClickAction(OnRightClickEquipment);
        accessoryEventButton.AddPointerRightClickAction(OnRightClickEquipment);
        //weaponEventButton.AddPointerLeftClickAction(OnLeftClickEquipment);

        opinionsEventLabel.SetShouldColorHighlight(false);
        statusTraitsEventLbl.SetShouldColorHighlight(false);
        normalTraitsEventLbl.SetShouldColorHighlight(false);

        moodMeter.ResetMarks();
        moodMeter.AddMark(EditableValuesManager.Instance.criticalMoodHighThreshold/100f, Color.red);
        moodMeter.AddMark(EditableValuesManager.Instance.lowMoodHighThreshold/100f, Color.yellow);

        Color green = Color.green;//new Color(0f / 255f, 91f / 255f, 0f / 255f);
        
        energyMeter.ResetMarks();
        energyMeter.AddMark(CharacterNeedsComponent.REFRESHED_LOWER_LIMIT/100f, green);
        energyMeter.AddMark(CharacterNeedsComponent.TIRED_UPPER_LIMIT/100f, Color.yellow);
        energyMeter.AddMark(CharacterNeedsComponent.EXHAUSTED_UPPER_LIMIT/100f, Color.red);
        
        fullnessMeter.ResetMarks();
        fullnessMeter.AddMark(CharacterNeedsComponent.FULL_LOWER_LIMIT/100f, green);
        fullnessMeter.AddMark(CharacterNeedsComponent.HUNGRY_UPPER_LIMIT/100f, Color.yellow);
        fullnessMeter.AddMark(CharacterNeedsComponent.STARVING_UPPER_LIMIT/100f, Color.red);
        
        happinessMeter.ResetMarks();
        happinessMeter.AddMark(CharacterNeedsComponent.BORED_UPPER_LIMIT/100f, Color.yellow);
        happinessMeter.AddMark(CharacterNeedsComponent.SULKING_UPPER_LIMIT/100f, Color.red);
        
        // staminaMeter.ResetMarks();
        // staminaMeter.AddMark(CharacterNeedsComponent.SPRIGHTLY_LOWER_LIMIT/100f, Color.green);
        // staminaMeter.AddMark(CharacterNeedsComponent.SPENT_UPPER_LIMIT/100f, Color.yellow);
        // staminaMeter.AddMark(CharacterNeedsComponent.DRAINED_UPPER_LIMIT/100f, Color.red);
        
        // hopeMeter.ResetMarks();
        // hopeMeter.AddMark(CharacterNeedsComponent.HOPEFUL_LOWER_LIMIT/100f, Color.green);
        // hopeMeter.AddMark(CharacterNeedsComponent.DISCOURAGED_UPPER_LIMIT/100f, Color.yellow);
        // hopeMeter.AddMark(CharacterNeedsComponent.HOPELESS_UPPER_LIMIT/100f, Color.red);

        _logsWindow.Initialize();

        InitializeRelationships();
        
        afflictions = new List<SkillData>();
        _dictMoodSummary = new Dictionary<string, MoodSummaryEntry>();

        piercingAndResistancesInfo.Initialize();
        SetButtonRevealPriceDisplay();

        ListenTalentHoverListener();
        ListenEquipmentHoverListener();
    }

    private void OnPlaguePointsAdjusted(int p_amount, int p_plaguePoints) {
        InitializeRevealHoverText();
    }

    void SetButtonRevealPriceDisplay() {
        btnRevealInfo.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
        btnRevealLogs.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
        btnRevealMood.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
        btnRevealRelationship.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
    }

    private void InitializeRevealHoverText() {
        if (PlayerManager.Instance.player.plagueComponent.plaguePoints < EditableValuesManager.Instance.GetRevealCharacterInfoCost()) {
            btnRevealInfo.GetComponent<HoverText>()?.SetText("Not Enough Chaotic Energy");
            btnRevealLogs.GetComponent<HoverText>()?.SetText("Not Enough Chaotic Energy");
            btnRevealMood.GetComponent<HoverText>()?.SetText("Not Enough Chaotic Energy");
            btnRevealRelationship.GetComponent<HoverText>()?.SetText("Not Enough Chaotic Energy");
            btnRevealInfo.GetComponent<RuinarchButton>().MakeUnavailable();
            btnRevealLogs.GetComponent<RuinarchButton>().MakeUnavailable();
            btnRevealMood.GetComponent<RuinarchButton>().MakeUnavailable();
            btnRevealRelationship.GetComponent<RuinarchButton>().MakeUnavailable();
        } else {
            btnRevealInfo.GetComponent<HoverText>()?.SetText("Reveal Character Info");
            btnRevealLogs.GetComponent<HoverText>()?.SetText("Reveal Character Info");
            btnRevealMood.GetComponent<HoverText>()?.SetText("Reveal Character Info");
            btnRevealRelationship.GetComponent<HoverText>()?.SetText("Reveal Character Info");
            btnRevealInfo.GetComponent<RuinarchButton>().MakeAvailable();
            btnRevealLogs.GetComponent<RuinarchButton>().MakeAvailable();
            btnRevealMood.GetComponent<RuinarchButton>().MakeAvailable();
            btnRevealRelationship.GetComponent<RuinarchButton>().MakeAvailable();
        }
    }

    private void CharacterRequestedForUpdate(Character p_character) {
        if (isShowing && _activeCharacter == p_character) {
            UpdateCharacterInfo();
        }
    }
    
    private void OnEquipmentUnequipped(Character p_character, EquipmentItem p_unequipped) {
        if (isShowing && activeCharacter == p_character) {
            UpdateEquipmentDisplay();
        }
    }
    
    private void OnReceiveKeyCodeSignal(KeyCode p_key) {
        if (p_key == KeyCode.Mouse1) {
            CloseMenu();
        }
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        Character character = _activeCharacter;
        _activeCharacter = null;
        if (character != null && ReferenceEquals(character.marker, null) == false) {
            if (InnerMapCameraMove.Instance != null && InnerMapCameraMove.Instance.target == character.marker.gameObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
            character.marker.UpdateNameplateElementsState();
        }
        m_currentViewMode = VIEW_MODE.None;
    }
    public override void OpenMenu() {
        _previousCharacter = _activeCharacter;
        _activeCharacter = _data as Character;
        base.OpenMenu();
        InitializeRevealHoverText();
        piercingAndResistancesInfo.UpdatePierceUI(_activeCharacter);
        if (_previousCharacter != null && _previousCharacter.hasMarker) {
            bool updateNameplate = _previousCharacter.grave == null || _previousCharacter.grave.isBeingCarriedBy == null;
            if (updateNameplate) {
                _previousCharacter.marker.UpdateNameplateElementsState();    
            }
        }
        if (UIManager.Instance.IsConversationMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        if (_activeCharacter.marker && _activeCharacter.marker.transform != null) {
            if (_activeCharacter.tileObjectComponent.isUsingBed) {
                if (_activeCharacter.tileObjectComponent.bedBeingUsed.mapObjectVisual) {
                    Selector.Instance.Select(_activeCharacter.tileObjectComponent.bedBeingUsed, _activeCharacter.tileObjectComponent.bedBeingUsed.mapObjectVisual.transform);
                }
            } else {
                Selector.Instance.Select(_activeCharacter, _activeCharacter.marker.transform);
            }
            
            //if character has no grave or grave is not being carried by anyone, then update nameplate elements
            bool updateNameplate = _activeCharacter.grave == null || _activeCharacter.grave.isBeingCarriedBy == null; 
            if (updateNameplate) {
                _activeCharacter.marker.UpdateNameplateElementsState();    
            }
        }
        btnStoreTarget.SetTarget(_activeCharacter);
        UpdateCharacterInfo();
        UpdateTraits();
        UpdateRelationships();
        UpdateInventoryInfo();
        _logsWindow.OnParentMenuOpened(_activeCharacter.persistentID);
        UpdateAllHistoryInfo();
        ResetAllScrollPositions();
        UpdateMoodSummary();
        ProcessDisplay();
    }
    #endregion

    void ProcessDisplay() {
        switch (m_currentViewMode) {
            case VIEW_MODE.Info:    
            OnToggleInfo(true);
            break;
            case VIEW_MODE.Mood:
            OnToggleMood(true);
            break;
            case VIEW_MODE.Relationship:
            OnToggleRelations(true);
            break;
            case VIEW_MODE.Logs:
            OnToggleLogs(true);
            break;
            default:
            OnToggleInfo(false);
            OnToggleMood(false);
            OnToggleRelations(false);
            OnToggleInfo(true);
            break;
        }
        InitializeRevealHoverText();
    }

    #region Utilities
    private void ResetAllScrollPositions() {
        _logsWindow.ResetScrollPosition();
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
        UpdatePartyInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
    }
    private void OnCharacterChangedName(Character p_character) {
        if (isShowing) {
            //update all basic info regardless of character since changed character might be referenced in active characters thought bubble.
            UpdateBasicInfo();    
        }
    }
    private void OnCharacterChangedClass(Character p_character, CharacterClass p_previousClass, CharacterClass p_newClass) {
        if (isShowing && activeCharacter == p_character) {
            UpdateBasicInfo();
        }
    }
    public void UpdateBasicInfo() {
        nameLbl.text = $"<b>{_activeCharacter.firstNameWithColor}</b>";
        UpdateSubTextAndIcon();
        UpdateThoughtBubble();
    }
    private void UpdateSubTextAndIcon() {
        if (!_activeCharacter.isNormalCharacter) {
            subLbl.gameObject.SetActive(false);
        } else {
            subLbl.text = _activeCharacter.characterClass.className;
            raceIcon.sprite = _activeCharacter.raceSetting.nameplateIcon;
            raceIcon.gameObject.SetActive(_activeCharacter.raceSetting.nameplateIcon != null);
            subLbl.gameObject.SetActive(true);
        }
    }
    public void UpdateThoughtBubble() {
        actionLbl.text = activeCharacter.visuals.GetThoughtBubble();
    }
    private void OnRightClickThoughtBubble(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = $"{_activeCharacter.currentHP.ToString()}/{_activeCharacter.maxHP.ToString()}";
        attackLbl.text = $"{_activeCharacter.combatComponent.GetComputedStrength().ToString()}";
        speedLbl.text =  $"{_activeCharacter.combatComponent.attackSpeed / 1000f}s";
        coinsLbl.text = $"{_activeCharacter.moneyComponent.coins.ToString()}";
        intLbl.text = $"{_activeCharacter.combatComponent.GetComputedIntelligence().ToString()}";
        critRateLbl.text = $"{_activeCharacter.combatComponent.critRate.ToString()}%";
        raceLbl.text = $"{GameUtilities.GetNormalizedSingularRace(_activeCharacter.race)}";
        elementLbl.text = $"<size=\"20\">{UtilityScripts.Utilities.GetRichTextIconForElement(_activeCharacter.combatComponent.elementalDamage.type)}</size>"; // + $"{_activeCharacter.combatComponent.elementalDamage.type}"
        piercingLbl.text = $"{_activeCharacter.piercingAndResistancesComponent.piercingPower}<size=\"20\">{UtilityScripts.Utilities.PiercingIcon()}</size>";
        // titlePiercingLbl.text = $"Piercing{UtilityScripts.Utilities.ColorizeSpellTitle($"{UtilityScripts.Utilities.PiercingIcon()}")}";
    }
    #endregion

    #region Location
    private void UpdateLocationInfo() {
        factionLbl.text = _activeCharacter.faction != null ? $"<link=\"faction\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.faction.name, FactionManager.Instance.GetFactionNameColorHex())}</link>" : "Factionless";
        currentLocationLbl.text = _activeCharacter.structureComponent.workPlaceStructure != null ? $"{_activeCharacter.structureComponent.workPlaceStructure.name}" : "None";
        homeRegionLbl.text = _activeCharacter.homeSettlement != null ? $"<link=\"home\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.homeSettlement.name)}</link>" : "Homeless";
        houseLbl.text = _activeCharacter.homeStructure != null ? $"<link=\"house\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.homeStructure.name)}</link>" : "Homeless";

        UpdateEquipmentDisplay();
        UpdatetalentsDisplay();
    }

    private void UpdateEquipmentDisplay() {
        if (_activeCharacter.equipmentComponent.currentWeapon != null) {
            weaponImg.enabled = true;
            weaponImg.sprite = _activeCharacter.equipmentComponent.currentWeapon.equipmentData.imgIcon;
            //weaponHoverText.Enable();
            //weaponHoverText.SetText(_activeCharacter.equipmentComponent.currentWeapon.name + "\n\n" + _activeCharacter.equipmentComponent.currentWeapon.GetBonusDescription());
            weaponEventButton.SetData(_activeCharacter, _activeCharacter.equipmentComponent.currentWeapon);
        } else {
            //weaponHoverText.Disable();
            weaponImg.enabled = false;
            weaponEventButton.ClearData();
        }
        if (_activeCharacter.equipmentComponent.currentArmor != null) {
            armorImg.enabled = true;
            armorImg.sprite = _activeCharacter.equipmentComponent.currentArmor.equipmentData.imgIcon;
            //armorHoverText.Enable();
            //armorHoverText.SetText(_activeCharacter.equipmentComponent.currentArmor.name + "\n\n" + _activeCharacter.equipmentComponent.currentArmor.GetBonusDescription());
            armorEventButton.SetData(_activeCharacter, _activeCharacter.equipmentComponent.currentArmor);
        } else {
            //armorHoverText.Disable();
            armorImg.enabled = false;
            armorEventButton.ClearData();
        }
        if (_activeCharacter.equipmentComponent.currentAccessory != null) {
            accessoryImg.enabled = true;
            accessoryImg.sprite = _activeCharacter.equipmentComponent.currentAccessory.equipmentData.imgIcon;
            //accessoryHoverText.Enable();
            //accessoryHoverText.SetText(_activeCharacter.equipmentComponent.currentAccessory.name + "\n\n" + _activeCharacter.equipmentComponent.currentAccessory.GetBonusDescription());
            accessoryEventButton.SetData(_activeCharacter, _activeCharacter.equipmentComponent.currentAccessory);
        } else {
            //accessoryHoverText.Disable();
            accessoryImg.enabled = false;
            accessoryEventButton.ClearData();
        }
    }

    private void OnHoverExitEquipment() {
        equipmentToolTip.gameObject.SetActive(false);
    }

    private void OnHoverExitTalent() {
        talentToolTip.gameObject.SetActive(false);
    }

    private void OnHoverEquipment(EquipmentItem p_equipmentItem) {
        if (p_equipmentItem != null) {
            equipmentToolTip.gameObject.SetActive(true);
            equipmentToolTip.ShowEquipmentItem(p_equipmentItem);
        }
    }

    private void OnHoverTalent(Character p_character, CHARACTER_TALENT p_talentType) {
        talentToolTip.gameObject.SetActive(true);
        talentToolTip.ShowCharacterTalentData(activeCharacter, p_talentType);
    }

    private void ListenTalentHoverListener() {
        martialArtsHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Martial_Arts));
        combatMagicHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Combat_Magic));
        healingMagicHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Healing_Magic));
        craftingHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Crafting));
        resourcesHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Resources));
        foodHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Food));
        socialHoverText.AddOnHoverOverAction(() => OnHoverTalent(activeCharacter, CHARACTER_TALENT.Social));

        martialArtsHoverText.AddOnHoverOutAction(OnHoverExitTalent);
        combatMagicHoverText.AddOnHoverOutAction(OnHoverExitTalent);
        healingMagicHoverText.AddOnHoverOutAction(OnHoverExitTalent);
        craftingHoverText.AddOnHoverOutAction(OnHoverExitTalent);
        resourcesHoverText.AddOnHoverOutAction(OnHoverExitTalent);
        foodHoverText.AddOnHoverOutAction(OnHoverExitTalent);
        socialHoverText.AddOnHoverOutAction(OnHoverExitTalent);
    }

    private void ListenEquipmentHoverListener() {
        weaponHoverText.AddOnHoverOverAction(() => OnHoverEquipment(activeCharacter.equipmentComponent.currentWeapon));
        armorHoverText.AddOnHoverOverAction(() => OnHoverEquipment(activeCharacter.equipmentComponent.currentArmor));
        accessoryHoverText.AddOnHoverOverAction(() => OnHoverEquipment(activeCharacter.equipmentComponent.currentAccessory));

        weaponHoverText.AddOnHoverOutAction(OnHoverExitEquipment);
        armorHoverText.AddOnHoverOutAction(OnHoverExitEquipment);
        accessoryHoverText.AddOnHoverOutAction(OnHoverExitEquipment);
    }

    private void UpdatetalentsDisplay() {
        martialArtsLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).level.ToString();
        CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Martial_Arts);
        

        combatMagicLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).level.ToString();
        talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Combat_Magic);
        

        healingMagicLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level.ToString();
        talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Healing_Magic);
        

        craftingLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Crafting).level.ToString();
        talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Crafting);
        

        resourcesLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Resources).level.ToString();
        talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Resources);
        

        foodLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Food).level.ToString();
        talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Food);
        

        socialLbl.text = _activeCharacter.talentComponent.GetTalent(CHARACTER_TALENT.Social).level.ToString();
        talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(CHARACTER_TALENT.Social);
        
    }

    private void OnClickFaction(object obj) {
        UIManager.Instance.ShowFactionInfo(activeCharacter.faction);
    }
    private void OnLeftClickHomeVillage(object obj) {
        if (_activeCharacter.homeSettlement != null) {
            UIManager.Instance.ShowSettlementInfo(_activeCharacter.homeSettlement);
        }
    }
    private void OnRightClickHomeVillage(object obj) { 
        if (_activeCharacter.homeSettlement != null) {
            UIManager.Instance.ShowPlayerActionContextMenu(_activeCharacter.homeSettlement, Input.mousePosition, true);
        }
    }
    private void OnLeftClickHomeStructure(object obj) {
        activeCharacter.homeStructure?.CenterOnStructure();
    }
    private void OnRightClickHomeStructure(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }
    #endregion

    #region Talents
    public void ToggleTalents() {
        talentsParentObject.SetActive(!talentsParentObject.activeSelf);
    }
	#endregion

	#region Traits
	private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
        UpdateStatInfo();
    }
    private void UpdateThoughtBubbleFromSignal(Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateThoughtBubble();
        }
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;
        bool isNormalCharacter = _activeCharacter.isNormalCharacter;

        for (int i = 0; i < _activeCharacter.traitContainer.statuses.Count; i++) {
            Status currStatus = _activeCharacter.traitContainer.statuses[i];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // if (currStatus.isHidden) {
            //     continue; //skip
            // }
            if (currStatus.IsNeeds() && !isNormalCharacter) {
                continue; //skip
            }
#else
            if (currStatus.isHidden) {
                continue; //skip
            }
            if (currStatus.IsNeeds() && !isNormalCharacter) {
                continue; //skip
            }
#endif
            string color = UIManager.normalTextColor;
            if (currStatus.moodEffect > 0 || currStatus.effect == TRAIT_EFFECT.POSITIVE) {
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
            if (currTrait.IsNeeds() && !isNormalCharacter) {
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
            Trait trait = activeCharacter.traitContainer.traits.ElementAtOrDefault(index);
            if (trait != null) {
                string info = trait.descriptionInUI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                info += $"\n{trait.GetTestingData(activeCharacter)}";
#endif
                UIManager.Instance.ShowSmallInfo(info, autoReplaceText:false);    
            }
        }
    }
    public void OnHoverStatus(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeCharacter.traitContainer.statuses.ElementAtOrDefault(index);
            if (trait != null) {
                string info = trait.descriptionInUI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                info += $"\n{trait.GetTestingData(activeCharacter)}";
#endif
                UIManager.Instance.ShowSmallInfo(info, autoReplaceText:false);    
            }
        }
    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(TileObject item, Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateInventoryInfo();
        }
    }
    private void OnLeftClickItem(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            TileObject tileObject = _activeCharacter.items.ElementAtOrDefault(index);
            if (tileObject != null) {
                UIManager.Instance.ShowTileObjectInfo(tileObject);    
            }
        }
    }
    private void OnRightClickItem(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            TileObject tileObject = _activeCharacter.items.ElementAtOrDefault(index);
            if (tileObject != null) {
                UIManager.Instance.ShowPlayerActionContextMenu(tileObject, Input.mousePosition, true);    
            }
        }
    }
    private void OnLeftClickEquipment(Character p_owner, TileObject targetWeapon) {
        //TileObject tileObject = p_owner.equipmentInventory.ElementAtOrDefault(index);
        if (targetWeapon != null) {
            UIManager.Instance.ShowTileObjectInfo(targetWeapon);
        }
    }
    private void OnRightClickEquipment(Character p_owner, TileObject targetWeapon) {
        //TileObject tileObject = _activeCharacter.equipmentInventory.ElementAtOrDefault(index);
        if (targetWeapon != null) {
            UIManager.Instance.ShowPlayerActionContextMenu(targetWeapon, Input.mousePosition, true);
        }
    }
    private void UpdateInventoryInfo() {
        itemsLbl.text = string.Empty;
        for (int i = 0; i < _activeCharacter.items.Count; i++) {
            TileObject currInventoryItem = _activeCharacter.items[i];
            itemsLbl.text = $"{itemsLbl.text}<link=\"{i.ToString()}\">{UtilityScripts.Utilities.ColorizeAndBoldName(currInventoryItem.name)}</link>";
            if (i < _activeCharacter.items.Count - 1) {
                itemsLbl.text = $"{itemsLbl.text}, ";
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(Log log) {
        if (isShowing && log.IsInvolved(_activeCharacter)) {
            UpdateAllHistoryInfo();
        }
    }
    private void OnLogMentioningCharacterUpdated(Character character) {
        if (isShowing) {
            //update history regardless of character because updated character might be referenced in this characters logs
            UpdateAllHistoryInfo();
        }
    }
    public void UpdateAllHistoryInfo() {
        _logsWindow.UpdateAllHistoryInfo();
    }
    #endregion   

    #region Listeners
    private void OnOpenConversationMenu() {
        backButton.interactable = false;
    }
    private void OnCharacterDied(Character character) {
        if (isShowing) {
            if (activeCharacter.id == character.id) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
                UpdateMoodSummary();
            }
            if (activeCharacter.relationshipContainer.HasRelationshipWith(character)) {
                UpdateRelationships();
            }
        }
    }
    #endregion

    #region For Testing
    public void ShowCharacterTestingInfo() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        TestingUtilities.ShowCharacterTestingInfo(activeCharacter);
#endif
    }
    public void HideCharacterTestingInfo() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        TestingUtilities.HideCharacterTestingInfo();
#endif
    }
    #endregion

    #region Relationships
    private void InitializeRelationships() {
        for (int i = 0; i < relationFilterItems.Length; i++) {
            RelationshipFilterItem item = relationFilterItems[i];
            item.Initialize(OnToggleRelationshipFilter);
            item.SetIsOnWithoutNotify(true);
        }
        allRelationshipFiltersToggle.SetIsOnWithoutNotify(true);
        allFilters = CollectionUtilities.GetEnumValues<RELATIONS_FILTER>();
        filters = new List<RELATIONS_FILTER>(allFilters);
        aliveRelationsOnly = true;
    }
    public void OnToggleShowOnlyAliveRelations(bool isOn) {
        aliveRelationsOnly = isOn;
        UpdateRelationships();
    }
    public void OnToggleShowAll(bool isOn) {
        filters.Clear();
        if (isOn) {
            filters.AddRange(allFilters);
        }
        for (int i = 0; i < relationFilterItems.Length; i++) {
            RelationshipFilterItem item = relationFilterItems[i];
            item.SetIsOnWithoutNotify(isOn);
        }
        UpdateRelationships();
    }
    private void OnToggleRelationshipFilter(bool isOn, RELATIONS_FILTER filter) {
        if (isOn) {
            filters.Add(filter);
        } else {
            filters.Remove(filter);
        }
        allRelationshipFiltersToggle.SetIsOnWithoutNotify(filters.Count == allFilters.Length);
        UpdateRelationships();
    }
    public void ToggleRelationFilters() {
        relationFiltersGO.SetActive(!relationFiltersGO.activeSelf);
    }
    private void UpdateRelationships() {
        relationshipTypesLbl.text = string.Empty;
        relationshipNamesLbl.text = string.Empty;
        relationshipValuesLbl.text = string.Empty;
        
        HashSet<int> filteredKeys = new HashSet<int>();
        foreach (var kvp in activeCharacter.relationshipContainer.relationships) {
            if (DoesRelationshipMeetFilters(kvp.Key, kvp.Value)) {
                filteredKeys.Add(kvp.Key);
            }
        }
        
        Dictionary<int, IRelationshipData> orderedRels = _activeCharacter.relationshipContainer.relationships
            .OrderByDescending(k => k.Value.opinions.totalOpinion)
            .ToDictionary(k => k.Key, v => v.Value);
        List<int> allKeys = _activeCharacter.relationshipContainer.relationships.Keys.ToList();
        
        for (int i = 0; i < orderedRels.Keys.Count; i++) {
            int targetID = orderedRels.Keys.ElementAt(i);
            if (filteredKeys.Contains(targetID)) {
                int actualIndex = allKeys.IndexOf(targetID);
                IRelationshipData relationshipData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
                string relationshipName = _activeCharacter.relationshipContainer.GetRelationshipNameWith(targetID);
                Character target = CharacterManager.Instance.GetCharacterByID(targetID);
                
                relationshipTypesLbl.text += $"{relationshipName}\n";
            
                int opinionOfOther = 0;
                string opinionText;
                if (target != null && target.relationshipContainer.HasRelationshipWith(activeCharacter)) {
                    opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
                    opinionText = GetOpinionText(opinionOfOther);
                } else {
                    opinionText = "???";
                }
            
                relationshipNamesLbl.text += $"<link=\"{actualIndex.ToString()}\">{UtilityScripts.Utilities.ColorizeAndBoldName(relationshipData.targetName)}</link>\n";
                relationshipValuesLbl.text += $"<link=\"{actualIndex.ToString()}\">" +
                                              $"<color={BaseRelationshipContainer.OpinionColor(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}> " +
                                              $"{GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color> " +
                                              $"<color={BaseRelationshipContainer.OpinionColor(opinionOfOther)}>({opinionText})</color></link>\n";
            }
        }
        if (relationshipsScrollView.gameObject.activeInHierarchy) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(relationshipsScrollView.content);    
        }
    }
    private bool DoesRelationshipMeetFilters(int id, IRelationshipData data) {
        Character target = CharacterManager.Instance.GetCharacterByID(id);
        if (target != null) {
            if (aliveRelationsOnly && target.isDead) {
                return false;
            }
            return DoesRelationshipMeetAnyFilter(data);
        } else {
            //did not check aliveRelationsOnly because unspawned characters will be shown regardless
            return DoesRelationshipMeetAnyFilter(data);
        }
        return true;
    }
    private bool DoesRelationshipMeetAnyFilter(IRelationshipData data) {
        if (filters.Count == 0) {
            return false; //if no filters were provided, then HIDE all relationships
        }
        //loop through enabled filters, if relationships meets any filter then return true.
        bool hasMetAFilter = false;
        string opinionLabel = data.opinions.GetOpinionLabel();
        for (int i = 0; i < filters.Count; i++) {
            RELATIONS_FILTER filter = filters[i];
            switch (filter) {
                case RELATIONS_FILTER.Enemies:
                    if (opinionLabel == RelationshipManager.Enemy) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Rivals:
                    if (opinionLabel == RelationshipManager.Rival) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Acquaintances:
                    if (opinionLabel == RelationshipManager.Acquaintance) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Friends:
                    if (opinionLabel == RelationshipManager.Friend) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Close_Friends:
                    if (opinionLabel == RelationshipManager.Close_Friend) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Relatives:
                    if (data.IsFamilyMember()) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Lovers:
                    if (data.IsLover()) {
                        hasMetAFilter = true;
                    }
                    break;
            }
            if (hasMetAFilter) {
                return true;
            }
        }
        return false; //no filters were met.
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
    private void OnRelationshipChanged(Relatable owner, Relatable target) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void ShowOpinionData(int targetID) {
        IRelationshipData targetData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
        Character target = CharacterManager.Instance.GetCharacterByID(targetID);

        string summary = $"{activeCharacter.name}'s opinion of {targetData.targetName}";
        bool isPsychopath = activeCharacter.traitContainer.HasTrait("Psychopath");
        if (!isPsychopath) {
            summary += "\n---------------------";
            Dictionary<string, int> opinions = activeCharacter.relationshipContainer.GetOpinionData(targetID).allOpinions;
            foreach (KeyValuePair<string, int> kvp in opinions) {
                summary += $"\n{kvp.Key}: <color={BaseRelationshipContainer.OpinionColorNoGray(kvp.Value)}>{GetOpinionText(kvp.Value)}</color>";
            }
            summary += "\n---------------------";
        }
        summary +=
            $"\nTotal: <color={BaseRelationshipContainer.OpinionColorNoGray(targetData.opinions.totalOpinion)}>{GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color>";
        if (isPsychopath) {
            summary += $" (Psychopath)";
        }

        if (target != null) {
            int opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
            summary +=
                $"\n{targetData.targetName}'s opinion of {activeCharacter.name}: <color={BaseRelationshipContainer.OpinionColorNoGray(opinionOfOther)}>{GetOpinionText(opinionOfOther)}</color>";
        } else {
            summary += $"\n{targetData.targetName}'s opinion of {activeCharacter.name}: ???</color>";
        }
        
        summary +=
            $"\n\nCompatibility: {RelationshipManager.Instance.GetCompatibilityBetween(activeCharacter, targetID).ToString()}";
        summary +=
            $"\nState Awareness: {UtilityScripts.Utilities.NotNormalizedConversionEnumToString(targetData.awareness.state.ToString())}";
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
    private void OnLeftClickRelationship(object obj) {
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
    private void OnRightClickRelationship(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
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
            UIManager.Instance.ShowSmallInfo($"{relationshipData.targetName} is not yet in this region or is already gone.", relationshipNameplateItemPosition);
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
        List<PLAYER_SKILL_TYPE> afflictionTypes = PlayerManager.Instance.player.playerSkillComponent.afflictions;
        for (int i = 0; i < afflictionTypes.Count; i++) {
            PLAYER_SKILL_TYPE spellType = afflictionTypes[i];
            SkillData spellData = PlayerSkillManager.Instance.GetSkillData(spellType);
            afflictions.Add(spellData);
        }
        UIManager.Instance.ShowClickableObjectPicker(afflictions, ActivateAfflictionConfirmation, null, CanActivateAffliction,
            "Select Affliction", OnHoverAffliction, OnHoverOutAffliction, 
            portraitGetter: null, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true);
    }
    private Sprite GetAfflictionPortrait(string str) {
        return PlayerManager.Instance.GetJobActionSprite(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(str));
    }
    private void ActivateAfflictionConfirmation(object o) {
        SkillData affliction = (SkillData)o;
        PLAYER_SKILL_TYPE afflictionType = affliction.type;
        string afflictionName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(afflictionType.ToString());
        UIManager.Instance.ShowYesNoConfirmation("Affliction Confirmation",
            "Are you sure you want to afflict " + afflictionName + "?", () => ActivateAffliction(afflictionType),
            layer: 26, showCover: true, pauseAndResume: true);
    }
    private void ActivateAffliction(PLAYER_SKILL_TYPE afflictionType) {
        UIManager.Instance.HideObjectPicker();
        PlayerSkillManager.Instance.GetAfflictionData(afflictionType).ActivateAbility(activeCharacter);
        PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.AFFLICT).OnExecutePlayerSkill();
    }
    private bool CanActivateAffliction(SkillData spellData) {
        return spellData.CanPerformAbilityTowards(activeCharacter);
    }
    private void OnHoverAffliction(SkillData spellData) {
        PlayerUI.Instance.OnHoverSpell(spellData);
    }
    private void OnHoverOutAffliction(SkillData spellData) {
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
        UtilityScripts.Utilities.DestroyChildren(scrollViewMoodSummary.content);
        if (_activeCharacter.isDead) {
            //do not show mood thoughts of dead character
            return;
        }
        _dictMoodSummary.Clear();
        foreach (var modification in _activeCharacter.moodComponent.allMoodModifications) {
            MoodModification moodModification = modification.Value;
            for (int i = 0; i < moodModification.flavorTexts.Count; i++) {
                Log flavorLog = moodModification.flavorTexts[i];
                if (flavorLog == null) { continue; }
                int modificationAmount = moodModification.modifications[i];
                GameDate expiryDate = moodModification.expiryDates.ElementAtOrDefault(moodModification.expiryDates.Count - 1 - i);
                MoodSummaryEntry moodSummaryEntry;
                if (!_dictMoodSummary.ContainsKey(flavorLog.logText)) {
                    moodSummaryEntry = new MoodSummaryEntry() { amount = modificationAmount, expiryDate = expiryDate };
                    _dictMoodSummary.Add(flavorLog.logText, moodSummaryEntry);
                } else {
                    moodSummaryEntry = _dictMoodSummary[flavorLog.logText];
                    moodSummaryEntry.amount += modificationAmount;
                    if (expiryDate.IsAfter(moodSummaryEntry.expiryDate)) {
                        moodSummaryEntry.expiryDate = expiryDate;    
                    }
                    _dictMoodSummary[flavorLog.logText] = moodSummaryEntry;
                }
            }
        }
        _dictMoodSummary = _dictMoodSummary.OrderByDescending(x => x.Value.amount).ToDictionary(k => k.Key, v => v.Value);
        foreach (var mood in _dictMoodSummary) {
            GameObject moodItemGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabMoodThought.name, Vector3.zero, Quaternion.identity, scrollViewMoodSummary.content);
            MoodThoughtUIItem moodItem = moodItemGO.GetComponent<MoodThoughtUIItem>();
            moodItem.SetItemDetails(mood.Key, mood.Value.amount, mood.Value.expiryDate, OnHoverOverMoodEffect, OnHoverOutMoodEffect);
        }
        // moodSummary.text = string.Empty;
        // string summary = string.Empty;
        // int index = 0;
        // foreach (KeyValuePair<string, int> pair in _activeCharacter.moodComponent.moodModificationsSummary) {
        //     string color = "green";
        //     string text = "+" + pair.Value;
        //     if (pair.Value < 0) {
        //         color = "red";
        //         text = pair.Value.ToString();
        //     }
        //     summary += $"<color={color}>{text}</color> <link=\"{index}\">{pair.Key}</link>\n";
        //     index++;
        // }
        // moodSummary.text = summary;
    }
    private void OnHoverOverMoodEffect(GameDate expiryDate) {
        string expiryText;
        if (expiryDate.hasValue) {
            GameDate today = GameManager.Instance.Today();
            expiryText = $"Lasts for: {today.GetTimeDifferenceString(expiryDate)}";
        } else {
            expiryText = "Lasts until: Linked to Needs";
        }
        UIManager.Instance.ShowSmallInfo(expiryText, autoReplaceText: false);
    }
    public void OnHoverMoodEffect(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            // if (index < _activeCharacter.moodComponent.allMoodModifications.Count) {
            //     var kvp = _activeCharacter.moodComponent.allMoodModifications.ElementAt(index);
            //     MoodModification modifications = kvp.Value;
            //     int total = _activeCharacter.moodComponent.moodModificationsSummary[kvp.Key];
            //     string modificationSign = string.Empty;
            //     if (total > 0) {
            //         modificationSign = "+";
            //     }
            //     string color = "green";
            //     if (total < 0) {
            //         color = "red";
            //     }
            //     GameDate expiryDate = modifications.expiryDates.Last();
            //     string expiryText;
            //     if (expiryDate.hasValue) {
            //         GameDate today = GameManager.Instance.Today();
            //         expiryText = $"Lasts for: {today.GetTimeDifferenceString(expiryDate)}";
            //     } else {
            //         expiryText = "Lasts until: Linked to Needs";
            //     }
            //     string summary = $"<color={color}>{modificationSign}{total.ToString()}</color> {expiryText}";
            //     UIManager.Instance.ShowSmallInfo(summary, autoReplaceText: false);    
            // }
        }
    }
    public void OnHoverOutMoodEffect() {
        UIManager.Instance.HideSmallInfo();
    }
    public void ShowMoodTooltip() {
        string summary = $"Represents the Villagers' overall state of mind. Lower a Villagers' Mood to make them less effective and more volatile.\n\n" +
                         $"{_activeCharacter.moodComponent.moodValue.ToString()}/100\nBrainwash Success Rate: {PrisonCell.GetBrainwashSuccessRate(_activeCharacter).ToString("N0")}%";
        UIManager.Instance.ShowSmallInfo(summary, $"MOOD: {_activeCharacter.moodComponent.moodStateName}");
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
        // hopeMeter.SetFillAmount(_activeCharacter.needsComponent.hope/CharacterNeedsComponent.HOPE_DEFAULT);
        // staminaMeter.SetFillAmount(_activeCharacter.needsComponent.stamina/CharacterNeedsComponent.STAMINA_DEFAULT);
    }
    public void ShowEnergyTooltip() {
#if UNITY_EDITOR
        string summary = $"Villagers will become Unconscious once this Meter is empty. This is replenished through rest.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.tiredness.ToString()}/100";
#else
        string summary = $"Villagers will become Unconscious once this Meter is empty. This is replenished through rest.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.tiredness.ToString("N0")}/100";
#endif
        UIManager.Instance.ShowSmallInfo(summary, "ENERGY");
    }
    public void ShowFullnessTooltip() {
#if UNITY_EDITOR
        string summary = $"Villagers will become Malnourished and eventually die once this Meter is empty. This is replenished through eating.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.fullness.ToString()}/100";
#else
        string summary = $"Villagers will become Malnourished and eventually die once this Meter is empty. This is replenished through eating.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.fullness.ToString("N0")}/100";
#endif
        UIManager.Instance.ShowSmallInfo(summary, "FULLNESS");
    }
    public void ShowHappinessTooltip() {
#if UNITY_EDITOR
        string summary = $"Villager's Mood becomes significantly affected when this Meter goes down. This is replenished by doing fun activities.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.happiness.ToString()}/100";
#else
        string summary = $"Villager's Mood becomes significantly affected when this Meter goes down. This is replenished by doing fun activities.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.happiness.ToString("N0")}/100";
#endif
        
        UIManager.Instance.ShowSmallInfo(summary, "ENTERTAINMENT");
    }
    public void ShowHopeTooltip() {
#if UNITY_EDITOR
        string summary = $"How much this Villager trusts you. If this gets too low, they will be uncooperative towards you in various way.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.hope.ToString()}/100";
#else   
        string summary = $"How much this Villager trusts you. If this gets too low, they will be uncooperative towards you in various way.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.hope.ToString("N0")}/100";
#endif
        UIManager.Instance.ShowSmallInfo(summary, "TRUST");
    }
    public void ShowStaminaTooltip() {
#if UNITY_EDITOR
        string summary = $"Villagers will be unable to run when this Meter is empty. This is used up when the Villager is running and quickly replenished when he isn't.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.stamina.ToString()}/100";
#else
        string summary = $"Villagers will be unable to run when this Meter is empty. This is used up when the Villager is running and quickly replenished when he isn't.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.stamina.ToString("N0")}/100";
#endif
        UIManager.Instance.ShowSmallInfo(summary, "STAMINA");
    }
    #endregion

    #region pierce UI
    public void ClickPierce() {
        if (pierceUI.isShowing) {
            pierceUI.HidePiercingAndResistancesInfo();
        } else {
            pierceUI.ShowPiercingAndResistancesInfo(activeCharacter);
        }
    }

    public void HidePierceUI() {
        pierceUI.HidePiercingAndResistancesInfo();
    }
    #endregion

    #region Tabs
    public void OnRevealInfoclicked() {
        if (PlayerManager.Instance.player.plagueComponent.plaguePoints >= EditableValuesManager.Instance.GetRevealCharacterInfoCost()) {
            if (!activeCharacter.isInfoUnlocked) {
                activeCharacter.isInfoUnlocked = true;
                PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-EditableValuesManager.Instance.GetRevealCharacterInfoCost());
                ProcessDisplay();
                Messenger.Broadcast(CharacterSignals.CHARACTER_INFO_REVEALED);
            }
        }
    }

    void ShowInfoHideRevealButton() {
        btnRevealInfo.SetActive(false);
        infoContent.SetActive(true);
    }

    void ShowRevealButtonHideInfo() {
        btnRevealInfo.SetActive(true);
        infoContent.SetActive(false);
    }

    void ShowMoodHideRevealButton() {
        btnRevealMood.SetActive(false);
        moodContent.SetActive(true);
    }

    void ShowRevealButtonHideMood() {
        btnRevealMood.SetActive(true);
        moodContent.SetActive(false);
    }

    void ShowRelationshipHideRevealButton() {
        btnRevealRelationship.SetActive(false);
        relationshipContent.SetActive(true);
    }

    void ShowRevealButtonHideRelationship() {
        btnRevealRelationship.SetActive(true);
        relationshipContent.SetActive(false);
    }

    void ShowLogsHideRevealButton() {
        btnRevealLogs.SetActive(false);
        logContent.SetActive(true);
    }

    void ShowRevealButtonHideLogs() {
        btnRevealLogs.SetActive(true);
        logContent.SetActive(false);
    }

    void HideAllInfo() {
        ShowRevealButtonHideInfo();
        ShowRevealButtonHideMood();
        ShowRevealButtonHideRelationship();
        ShowRevealButtonHideLogs();
    }

    void ShowAllInfo() {
        ShowInfoHideRevealButton();
        ShowMoodHideRevealButton();
        ShowRelationshipHideRevealButton();
        ShowLogsHideRevealButton();
    }

    public void OnToggleInfo(bool isOn) {
        if (isOn) {
            m_currentViewMode = VIEW_MODE.Info;
            if (activeCharacter.race.IsSapient()) {
                if (activeCharacter.isInfoUnlocked) {
                    ShowAllInfo();
                } else {
                    HideAllInfo();
                }
            } else {
                ShowInfoHideRevealButton();
            }
        }
    }
    public void OnToggleMood(bool isOn) {
        if (isOn) {
            m_currentViewMode = VIEW_MODE.Mood;
            if (activeCharacter.race.IsSapient()) {
                if (activeCharacter.isInfoUnlocked) {
                    ShowAllInfo();
                } else {
                    HideAllInfo();
                }
            } else {
                ShowMoodHideRevealButton();
            }
        }
    }
    public void OnToggleRelations(bool isOn) {
        if (isOn) {
            m_currentViewMode = VIEW_MODE.Relationship;
            if (activeCharacter.race.IsSapient()) {
                if (activeCharacter.isInfoUnlocked) {
                    ShowAllInfo();
                } else {
                    HideAllInfo();
                }
            } else {
                ShowRelationshipHideRevealButton();
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(relationshipsScrollView.content);
    }
    public void OnToggleLogs(bool isOn) {
        if (isOn) {
            m_currentViewMode = VIEW_MODE.Logs;
            if (activeCharacter.race.IsSapient()) {
                if (activeCharacter.isInfoUnlocked) {
                    ShowAllInfo();
                } else {
                    HideAllInfo();
                }
            } else {
                ShowLogsHideRevealButton();
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(relationshipsScrollView.content);
    }
    #endregion

    #region Party
    private void UpdatePartyInfo() {
        string text = "None";
        if (activeCharacter.partyComponent.hasParty) {
            text = $"<link=\"party\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeCharacter.partyComponent.currentParty.partyName)}</link>";
        }
        partyLbl.text = text;
    }
    private void OnClickParty(object obj) {
        if (activeCharacter.partyComponent.hasParty) {
            UIManager.Instance.ShowPartyInfo(activeCharacter.partyComponent.currentParty);
        }
    }
    #endregion

    #region Rename
    public void OnClickRenameButton() {
        Messenger.Broadcast(UISignals.EDIT_CHARACTER_NAME, activeCharacter.persistentID, activeCharacter.firstName);
    }
    #endregion

    #region Race Icon
    public void OnHoverRaceIcon() {
        if(activeCharacter == null) {
            return;
        }
        string message = GameUtilities.GetNormalizedSingularRace(activeCharacter.race);
        if (activeCharacter.talentComponent != null) {
            message += "\n" + activeCharacter.talentComponent.GetTalentSummary();
        }
        UIManager.Instance.ShowSmallInfo(message);
    }
    public void OnHoverExitRaceIcon() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Piercing and Resistances
    public void TogglePiercingAndResistances() {
        if (piercingAndResistancesInfo.isShowing) {
            piercingAndResistancesInfo.HidePiercingAndResistancesInfo();
        } else {
            piercingAndResistancesInfo.ShowPiercingAndResistancesInfo(activeCharacter);
        }
    }
    #endregion

    private struct MoodSummaryEntry {
        public int amount;
        public GameDate expiryDate;
    }
}
