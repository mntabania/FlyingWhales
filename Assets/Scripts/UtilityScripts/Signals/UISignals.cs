public static class UISignals {
    public static string SHOW_POPUP_MESSAGE = "ShowPopupMessage"; //Parameters (string message, MESSAGE_BOX_MODE mode, bool expires)
    public static string HIDE_POPUP_MESSAGE = "HidePopupMessage";
    public static string UPDATE_UI = "UpdateUI";
    /// <summary>
    /// Parameters (string text, int expiry, UnityAction onClickAction)
    /// </summary>
    public static string SHOW_DEVELOPER_NOTIFICATION = "ShowNotification";
    public static string LOG_ADDED = "OnLogAdded"; //Parameters (object itemThatHadHistoryAdded) either a character or a landmark
    /// <summary>
    /// Parameters (Log)
    /// </summary>
    public static string LOG_IN_DATABASE_UPDATED = "OnLogInDatabaseUpdated";
    /// <summary>
    /// Parameters Log
    /// </summary>
    public static string LOG_REMOVED_FROM_DATABASE = "OnLogRemovedFromDatabase";
    public static string PAUSED = "OnPauseChanged"; //Parameters (bool isGamePaused)
    public static string PAUSED_BY_PLAYER = "OnPausedByPlayer";
    public static string PROGRESSION_SPEED_CHANGED = "OnProgressionSpeedChanged"; //Parameters (PROGRESSION_SPEED progressionSpeed)
    public static string BEFORE_MENU_OPENED = "BeforeMenuOpened"; //Parameters (UIMenu openedMenu)
    public static string MENU_OPENED = "OnMenuOpened"; //Parameters (UIMenu openedMenu)
    public static string MENU_CLOSED = "OnMenuClosed"; //Parameters (UIMenu closedMenu)
    public static string INTERACTION_MENU_OPENED = "OnInteractionMenuOpened"; //Parameters ()
    public static string INTERACTION_MENU_CLOSED = "OnInteractionMenuClosed"; //Parameters ()
    public static string DRAG_OBJECT_CREATED = "OnDragObjectCreated"; //Parameters (DragObject obj)
    public static string DRAG_OBJECT_DESTROYED = "OnDragObjectDestroyed"; //Parameters (DragObject obj)
    public static string SHOW_INTEL_NOTIFICATION = "ShowIntelNotification"; //Parameters (Intel)
    public static string SHOW_PLAYER_NOTIFICATION = "ShowPlayerNotification"; //Parameters (Log)
    public static string ON_SHARE_INTEL = "OnShareIntel";
    public static string ON_OPEN_CONVERSATION_MENU = "OnOpenConversationMenu";
    public static string ON_CLOSE_CONVERSATION_MENU = "OnOpenConversationMenu";
    public static string REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT = "OnAreaInfoUIUpdateAppropriateContent";
    public static string UPDATE_THOUGHT_BUBBLE = "OnUpdateThoughtBubble";
    /// <summary>
    /// Parameters: PopupMenuBase
    /// </summary>
    public static string POPUP_MENU_OPENED = "OnPopupMenuOpened";
    /// <summary>
    /// Parameters: PopupMenuBase
    /// </summary>
    public static string POPUP_MENU_CLOSED = "OnPopupMenuClosed";
    /// <summary>
    /// Parameters: string mainNameplateText
    /// </summary>
    public static string NAMEPLATE_CLICKED = "OnNameplateClicked";
    public static string SPELLS_MENU_SHOWN = "OnSpellsMenuShown";
    /// <summary>
    /// Parameters: Region selectedRegion
    /// </summary>
    public static string REGION_SELECTED = "OnRegionSelected";
    public static string FLAW_CLICKED = "OnFlawClicked";
    /// <summary>
    /// Parameters: Ruinarch.UI.RuinarchButton ClickedButton 
    /// </summary>
    public static string BUTTON_CLICKED = "OnButtonClicked";
    /// <summary>
    /// Parameters: Ruinarch.UI.RuinarchToggle ClickedToggle 
    /// </summary>
    public static string TOGGLE_CLICKED = "OnToggleClicked";
    /// <summary>
    /// Parameters: Ruinarch.UI.RuinarchToggle shownToggle 
    /// </summary>
    public static string TOGGLE_SHOWN = "OnToggleShown";
    /// <summary>
    /// Parameters: string sceneName
    /// </summary>
    public static string STARTED_LOADING_SCENE = "OnStartedLoadingScene";
    /// <summary>
    /// Parameters: string buttonName
    /// </summary>
    public static string SHOW_SELECTABLE_GLOW = "ShowSelectableGlow";
    /// <summary>
    /// Parameters: string buttonName
    /// </summary>
    public static string HIDE_SELECTABLE_GLOW = "HideSelectableGlow";
    /// <summary>
    /// Parameters: Quest quest
    /// </summary>
    public static string QUEST_SHOWN = "OnQuestShown";
    /// <summary>
    /// Parameters: Object, Log, Character
    /// </summary>
    public static string LOG_HISTORY_OBJECT_CLICKED = "OnLogObjectClicked";
    public static string SKILL_SLOT_ITEM_CLICKED = "OnSkillSlotItemClicked";
    public static string START_GAME_AFTER_LOADOUT_SELECT = "OnStartGameAfterLoadoutSelect";
    public static string UPDATE_BUILD_LIST = "UpdateBuildList";
    public static string RACE_WORLD_OPTION_ITEM_CLICKED = "OnRaceWorldOptionItemClicked";
    public static string BIOME_WORLD_OPTION_ITEM_CLICKED = "OnBiomeWorldOptionItemClicked";
    public static string UI_STATE_SET = "OnUIStateSet";
    /// <summary>
    /// Parameters: EventLabel
    /// </summary>
    public static string EVENT_LABEL_LINK_CLICKED = "OnEventLabelClicked";
    public static string UPDATE_FACTION_LOGS_UI = "UpdateFactionLogsUI";
    public static string UPDATE_POI_LOGS_UI = "UpdatePOILogsUI";
    /// <summary>
    /// Parameters: string buttonName
    /// </summary>
    public static string HOTKEY_CLICK = "OnHotKeyPressed";
    /// <summary>
    /// Parameters string deletedSavePath
    /// </summary>
    public static string SAVE_FILE_DELETED = "OnSaveFileDeleted";
    /// <summary>
    /// Parameters string fileToLoad
    /// </summary>
    public static string LOAD_SAVE_FILE = "OnLoadFileConfirmed";
    /// <summary>
    /// Parameters (Log)
    /// </summary>
    public static string LOG_MENTIONING_CHARACTER_UPDATED = "LogMentioningCharacterUpdated";
    public static string EDIT_CHARACTER_NAME = "OnEditCharacterName";
    /// <summary>
    /// Parameters (IIntel intel)
    /// </summary>
    public static string INTEL_LOG_UPDATED = "OnIntelLogUpdated";
    /// <summary>
    /// Parameters: string identifier
    /// </summary>
    public static string OBJECT_PICKER_SHOWN = "ObjectPickerShown";
    public static string INTEL_MENU_OPENED = "OnIntelMenuOpened";
    /// <summary>
    /// Parameters (QuestStep)
    /// </summary>
    public static string UPDATE_QUEST_STEP_ITEM = "UpdateQuestStepItem";
    public static string BUTTON_SHOWN = "OnButtonShown";
    /// <summary>
    /// Parameters (IPlayerActionTarget)
    /// </summary>
    public static string PLAYER_ACTION_CONTEXT_MENU_SHOWN = "OnPlayerActionContextMenuShown";
    public static string SCHEME_UI_SHOWN = "OnSchemeUIShown";
    public static string TEMPTATIONS_POPUP_SHOWN = "OnTemptationsPopupShown";
    public static string TEMPTATIONS_OFFERED = "OnTemptationsOffered";
    /// <summary>
    /// Parameters(TMP_Dropdown parentDropdown, int itemIndex)
    /// </summary>
    public static string DROPDOWN_ITEM_HOVERED_OVER = "OnDropdownItemHoveredOver";
    /// <summary>
    /// Parameters(TMP_Dropdown parentDropdown, int itemIndex)
    /// </summary>
    public static string DROPDOWN_ITEM_HOVERED_OUT = "OnDropdownItemHoveredOut";
    public static string SAVE_LOADOUTS = "SaveLoadouts";
    public static string UPDATE_PIERCING_AND_RESISTANCE_INFO = "UpdatePiercingAndResistanceInfo";
    /// <summary>
    /// Parameters: Character characterRequestingUpdate
    /// </summary>
    public static string UPDATE_CHARACTER_INFO = "UpdateCharacterInfo";
}