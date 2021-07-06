using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using DG.Tweening;
using Factions;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using JetBrains.Annotations;
using Logs;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UtilityScripts;
using Debug = UnityEngine.Debug;
using Prison = Tutorial.Prison;

public class UIManager : BaseMonoBehaviour {

    public Action onSpireClicked;
    public Action<LocationStructure> onMaraudClicked;
    public Action<LocationStructure> onKennelClicked;
    public Action<LocationStructure> onTortureChamberClicked;
    public Action<LocationStructure> onDefensePointClicked;

    public static UIManager Instance = null;

    public const string normalTextColor = "#CEB67C";
    public const string buffTextColor = "#39FF14";
    public const string flawTextColor = "#FF073A";

    public Canvas canvas;
    public RectTransform smallInfoCanvasRT;
    public RectTransform canvasRectTransform;
    private InfoUIBase[] allMenus;

    [Space(10)]
    [Header("Date Objects")]
    [SerializeField] private ToggleGroup speedToggleGroup;
    public Toggle pauseBtn;
    public Toggle x1Btn;
    public Toggle x2Btn;
    public Toggle x4Btn;
    [SerializeField] private TextMeshProUGUI dateLbl;
    
    [Space(10)]
    [Header("Small Info")]
    public GameObject smallInfoGO;
    public RectTransform smallInfoRT;
    public HorizontalLayoutGroup smallInfoBGParentLG;
    public VerticalLayoutGroup smallInfoVerticalLG;
    public RectTransform smallInfoBGRT;
    public RuinarchText smallInfoLbl;
    public LocationSmallInfo locationSmallInfo;
    public RectTransform locationSmallInfoRT;
    public GameObject characterPortraitHoverInfoGO;
    public CharacterPortrait characterPortraitHoverInfo;
    public RectTransform characterPortraitHoverInfoRT;
    public Canvas smallInfoCanvas;
    private List<int> cornersOutside = new List<int>();
    private Vector3[] cornerVectors = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right

    [Header("Small Info with Visual")] 
    [SerializeField] private SmallInfoWithVisual _smallInfoWithVisual;
    
    [Header("Character Nameplate Tooltip")]
    [SerializeField] private CharacterNameplateItem _characterNameplateTooltip;
    
    [Header("Tile Object Tooltip")]
    [SerializeField] private TileObjectNameplateItem _tileObjectNameplateTooltip;
    
    [Header("Tile Object Tooltip")]
    [SerializeField] private StructureNameplateItem _structureNameplateTooltip;
    
    [Header("Character Marker Nameplate")]
    public Transform characterMarkerNameplateParent;
    
    [Space(10)]
    [Header("Other NPCSettlement Info")]
    public Sprite[] areaCenterSprites;
    public GameObject portalPopup;
    
    [Space(10)]
    [Header("Notification NPCSettlement")]
    public DeveloperNotificationArea developerNotificationArea;

    [Space(10)]
    [Header("Shared")]
    [SerializeField] private GameObject cover;

    [Space(10)]
    [Header("Object Picker")]
    [SerializeField] private ObjectPicker objectPicker;
    
    [Space(10)]
    [Header("Right Click Commands")]
    public POITestingUI poiTestingUI;

    [Space(10)]
    [Header("Combat")]
    //public CombatUI combatUI;
    public CombatModeSpriteDictionary combatModeSpriteDictionary;

    [Space(10)]
    [Header("Nameplate Prefabs")]
    public GameObject characterNameplatePrefab;
    public GameObject stringNameplatePrefab;
    public GameObject worldEventNameplatePrefab;
    public GameObject factionNameplatePrefab;

    [Space(10)]
    [Header("Dual Object Picker")]
    public DualObjectPicker dualObjectPicker;

    [Space(10)]
    [Header("Psychopath")]
    public PsychopathUI psychopathUI;

    [Space(10)]
    [Header("Custom Dropdown List")]
    public CustomDropdownList customDropdownList;
    
    [Space(10)]
    [Header("Quest UI")]
    public QuestUI questUI;
    
    [Space(10)]
    [Header("Logs")]
    public LogTagSpriteDictionary logTagSpriteDictionary;
    
    [Space(10)]
    [Header("Load Window")]
    [SerializeField] private LoadWindow loadWindow;

    public InfoUIBase latestOpenedInfoUI { get; private set; }
    private InfoUIBase _lastOpenedInfoUI;
    private PointerEventData _pointer;
    private List<RaycastResult> _raycastResults;
    
    public bool tempDisableShowInfoUI { get; private set; }

    public List<UnallowOverlaps> unallowOverlaps = new List<UnallowOverlaps>();
    
    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        //openedPopups = new List<PopupMenuBase>();
        Messenger.AddListener<bool>(UISignals.PAUSED, UpdateSpeedToggles);
        Messenger.AddListener(UISignals.UPDATE_UI, UpdateUI);
    }
    private void Update() {
        if (Input.GetMouseButtonDown(0) && IsContextMenuShowing() && !IsMouseOnContextMenu()) { //!IsMouseOnUI() && GameManager.Instance.gameHasStarted
            HidePlayerActionContextMenu();
        }
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    #endregion
    
    internal void InitializeUI() {
        _pointer = new PointerEventData(EventSystem.current);
        _raycastResults = new List<RaycastResult>();
        allMenus = transform.GetComponentsInChildren<InfoUIBase>(true);
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
        }
        openedPopups = new List<PopupMenuBase>();
        questUI.Initialize();
        biolabUIController.Init(OnCloseBiolabUI);
        Messenger.AddListener<string, int, UnityAction>(UISignals.SHOW_DEVELOPER_NOTIFICATION, ShowDeveloperNotification);
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

        Messenger.AddListener(UISignals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(UISignals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);

        Messenger.AddListener<IIntel>(UISignals.SHOW_INTEL_NOTIFICATION, ShowPlayerNotification);
        Messenger.AddListener<Log>(UISignals.SHOW_PLAYER_NOTIFICATION, ShowPlayerNotification);

        Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
        Messenger.AddListener(UISignals.ON_CLOSE_CONVERSATION_MENU, OnCloseShareIntelMenu);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        
        Messenger.AddListener<InfoUIBase>(UISignals.MENU_OPENED, OnUIMenuOpened);
        Messenger.AddListener<InfoUIBase>(UISignals.MENU_CLOSED, OnUIMenuClosed);
        
        Messenger.AddListener<PopupMenuBase>(UISignals.POPUP_MENU_OPENED, OnPopupMenuOpened);
        Messenger.AddListener<PopupMenuBase>(UISignals.POPUP_MENU_CLOSED, OnPopupMenuClosed);
        
        Messenger.AddListener<IPointOfInterest>(UISignals.UPDATE_POI_LOGS_UI, TryUpdatePOILog);
        Messenger.AddListener<Faction>(UISignals.UPDATE_FACTION_LOGS_UI, TryUpdateFactionLog);

        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.AddListener<PlayerAction>(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, OnPlayerActionActivated);
        
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);

        AddPlayerActionContextMenuSignals();
        
        //notification area
        notificationSearchField.onValueChanged.AddListener(OnEndNotificationSearchEdit);
        notificationFilters = CollectionUtilities.GetEnumValues<LOG_TAG>().ToList();
        showAllToggle.SetIsOnWithoutNotify(true);
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem logFilterItem = allFilters[i];
            logFilterItem.SetOnToggleAction(OnToggleFilter);
            logFilterItem.SetIsOnWithoutNotify(true);
        }
        showAllToggle.onValueChanged.AddListener(OnToggleAllFilters);
        UpdateSearchFieldsState();
        
        contextMenuUIController.SetOnHoverOverAction(OnHoverOverPlayerActionContextMenuItem);
        contextMenuUIController.SetOnHoverOutAction(OnHoverOutPlayerActionContextMenuItem);
        optionsMenu.SubscribeListeners();
        
        UpdateUI();
    }
    public void InitializeAfterLoadOutPicked() {
        _portalUIController.InitializeAfterLoadoutSelected();
        upgradePortalUIController.InitializeAfterLoadoutSelected();
        purchaseSkillUIController.InitializeAfterLoadoutSelected();
    }
    private void OnPlayerActionActivated(PlayerAction p_playerAction) {
        if (p_playerAction.type == PLAYER_SKILL_TYPE.SEIZE_CHARACTER || p_playerAction.type == PLAYER_SKILL_TYPE.SEIZE_MONSTER || p_playerAction.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT
            || p_playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || p_playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW || p_playerAction.type == PLAYER_SKILL_TYPE.DESTROY || p_playerAction.type == PLAYER_SKILL_TYPE.DESTROY_EYE_WARD
            || p_playerAction.category == PLAYER_SKILL_CATEGORY.SCHEME
            || p_playerAction.type == PLAYER_SKILL_TYPE.PSYCHOPATHY
            || p_playerAction.type == PLAYER_SKILL_TYPE.SNATCH_MONSTER
            || p_playerAction.type == PLAYER_SKILL_TYPE.SNATCH_VILLAGER
            || p_playerAction.type == PLAYER_SKILL_TYPE.RAID
            || p_playerAction.type == PLAYER_SKILL_TYPE.DEFEND
            || p_playerAction.type == PLAYER_SKILL_TYPE.EVANGELIZE
            || p_playerAction.type == PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL
            || p_playerAction.type == PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL) {
            HidePlayerActionContextMenu();    
        } else {
            if (IsContextMenuShowing()) {
                ForceReloadPlayerActions();
            }    
        }
    }
    private void TryUpdateFactionLog(Faction faction) {
        if (factionInfoUI.isShowing && factionInfoUI.currentlyShowingFaction == faction) {
            factionInfoUI.UpdateAllHistoryInfo();
        }
    }
    private void TryUpdatePOILog(IPointOfInterest poi) {
        if (poi is Character) {
            if (characterInfoUI.isShowing) {
                characterInfoUI.UpdateAllHistoryInfo();
            }
        } else if (poi is TileObject) {
            if (tileObjectInfoUI.isShowing) {
                tileObjectInfoUI.UpdateLogs();
            }
        }
    }
    private void OnGameLoaded() {
        UpdateUI();
    }
    private void OnKeyPressed(KeyCode p_pressedKey) {
        if (p_pressedKey == KeyCode.F8) {
            OnLoadHotkeyPressed();
        }
    }
    private void UpdateUI() {
        dateLbl.SetText($"Day {GameManager.Instance.continuousDays.ToString()}\n{GameManager.Instance.ConvertTickToTime(GameManager.Instance.Today().tick)}");
        UpdateInteractableInfoUI();
        PlayerUI.Instance.UpdateUI();
    }
    private void UpdateInteractableInfoUI() {
        UpdateCharacterInfo();
        UpdateMonsterInfo();
        UpdateTileObjectInfo();
        UpdateStructureInfo();
        UpdateSettlementInfo();
        UpdatePartyInfo();
        UpdateUnbuiltStructureInfo();
    }

    #region World Controls
    private void UpdateSpeedToggles(bool isPaused) {
        if (!gameObject.activeInHierarchy) {
            return;
        }
        if (isPaused) {
            pauseBtn.isOn = true;
            speedToggleGroup.NotifyToggleOn(pauseBtn);
        } else {
            if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
                x1Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x1Btn);
                //SetProgressionSpeed1X();
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                x2Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x2Btn);
                //SetProgressionSpeed2X();
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                x4Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x4Btn);
                //SetProgressionSpeed4X();
            }
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED speed) {
        UpdateSpeedToggles(GameManager.Instance.isPaused);
    }
    public void SetProgressionSpeed1X() {
        if (!GameManager.Instance.isPaused && GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
            PauseByPlayer();
            return;
        }
        if (!x1Btn.IsInteractable()) {
            return;
        }
        Unpause();
        
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
    }
    public void SetProgressionSpeed2X() {
        if (!GameManager.Instance.isPaused && GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
            PauseByPlayer();
            return;
        }
        if (!x2Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
    }
    public void SetProgressionSpeed4X() {
        if (!GameManager.Instance.isPaused && GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
            PauseByPlayer();
            return;
        }
        if (!x4Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
    }
    /// <summary>
    /// Helper function to call if the player is the one that paused the game.
    /// </summary>
    public void PauseByPlayer() {
        if (GameManager.Instance.isPaused) {
            Unpause();
            return;
        }
        Pause();
        // Debug.Log("Game was paused by player.");
        Messenger.Broadcast(UISignals.PAUSED_BY_PLAYER);
    }
    public void Pause() {
        GameManager.Instance.SetPausedState(true);
    }
    public void Unpause() {
        GameManager.Instance.SetPausedState(false);
    }
    public void ShowDateSummary() {
        ShowSmallInfo(GameManager.Instance.Today().ToStringDate());
    }
    public void SetSpeedTogglesState(bool state) {
        pauseBtn.interactable = state;
        x1Btn.interactable = state;
        x2Btn.interactable = state;
        x4Btn.interactable = state;
    }
    /// <summary>
    /// Resume the last speed that the player was in before pausing the game.
    /// </summary>
    public void ResumeLastProgressionSpeed() {
        SetSpeedTogglesState(true);
        if (GameManager.Instance.lastProgressionBeforePausing == "paused") {
            //pause the game
            Pause();
        } else if (GameManager.Instance.lastProgressionBeforePausing == "1") {
            SetProgressionSpeed1X();
        } else if (GameManager.Instance.lastProgressionBeforePausing == "2") {
            SetProgressionSpeed2X();
        } else if (GameManager.Instance.lastProgressionBeforePausing == "4") {
            SetProgressionSpeed4X();
        }
    }
    #endregion

    #region Options
    [Header("Options")]
    [SerializeField] private OptionsMenu _optionsMenu;
    public OptionsMenu optionsMenu => _optionsMenu;
    public void ToggleOptionsMenu() {
        if (_optionsMenu.isShowing) {
            _optionsMenu.Close();
        } else {
            _optionsMenu.Open();
        }
    }
    public bool IsOptionsMenuShowing() {
        return _optionsMenu.isShowing;
    }
    public void OpenOptionsMenu() {
        _optionsMenu.Open();
    }
    public void CloseOptionsMenu() {
        _optionsMenu.Close();
    }
    #endregion

    #region Save Game
    [Header("Save Game")]
    [SerializeField] private GameObject _saveWritingToDiskGO;
    public void ShowSaveWritingToDisk() {
        _saveWritingToDiskGO.SetActive(true);
    }
    public void HideSaveWritingToDisk() {
        _saveWritingToDiskGO.SetActive(false);
    }
    #endregion

    #region Tooltips
    public void ShowSmallInfo(string info, string header = "", bool autoReplaceText = true) {
        smallInfoGO.transform.SetAsLastSibling();
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";
        
        message = message.Replace("\\n", "\n");
        
        if (autoReplaceText) {
            smallInfoLbl.SetTextAndReplaceWithIcons(message);    
        } else {
            smallInfoLbl.text = message;
        }
        if (!IsSmallInfoShowing()) {
            smallInfoGO.transform.SetParent(transform);
            smallInfoGO.SetActive(true);
            if (gameObject.activeInHierarchy) {
                StartCoroutine(ReLayout(smallInfoBGParentLG));
                StartCoroutine(ReLayout(smallInfoVerticalLG));    
            }
        }
        PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
    }
    public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "", bool autoReplaceText = true, bool relayout = false) {
        smallInfoGO.transform.SetAsLastSibling();
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";
        
        message = message.Replace("\\n", "\n");
        
        if (autoReplaceText) {
            smallInfoLbl.SetTextAndReplaceWithIcons(message);    
        } else {
            smallInfoLbl.text = message;
        }
        
        PositionTooltip(pos, smallInfoGO, smallInfoRT);
        
        if (!IsSmallInfoShowing()) {
            if (relayout) {
                smallInfoGO.SetActive(true);
                if (gameObject.activeInHierarchy) {
                    StartCoroutine(ReLayout(smallInfoBGParentLG));
                    StartCoroutine(ReLayout(smallInfoVerticalLG));    
                }
            }
            else {
                smallInfoGO.SetActive(true);
            }
        }
    }
    private IEnumerator ReLayout(LayoutGroup layoutGroup) {
        layoutGroup.enabled = false;
        yield return null;
        layoutGroup.enabled = true;
    }
    public void ShowSmallInfo(string info, [NotNull]VideoClip videoClip, string header = "", UIHoverPosition pos = null) {
        Assert.IsNotNull(videoClip, "Small info with visual was called but no video clip was provided");
        if (Settings.SettingsManager.Instance.doNotShowVideos) {
            ShowSmallInfo(info, pos, header);
        } else {
            _smallInfoWithVisual.ShowSmallInfo(info, videoClip, header, pos);
        }
    }
    public void ShowSmallInfo(string info, Texture visual, string header = "", UIHoverPosition pos = null) {
        Assert.IsNotNull(visual, "Small info with visual was called but no visual was provided");
        _smallInfoWithVisual.transform.SetAsLastSibling();
        _smallInfoWithVisual.ShowSmallInfo(info, visual, header, pos);
    }
    public void HideSmallInfo() {
        if (IsSmallInfoShowing()) {
            smallInfoGO.SetActive(false);
            _smallInfoWithVisual.Hide();
        }
    }
    public bool IsSmallInfoShowing() {
        return (smallInfoGO != null && smallInfoGO.activeSelf) || (_smallInfoWithVisual != null && _smallInfoWithVisual.gameObject.activeSelf);
    }
    public void ShowCharacterPortraitHoverInfo(Character character) {
        characterPortraitHoverInfo.GeneratePortrait(character);
        characterPortraitHoverInfoGO.SetActive(true);

        characterPortraitHoverInfoRT.SetParent(transform);
        PositionTooltip(characterPortraitHoverInfoRT.gameObject, characterPortraitHoverInfoRT, characterPortraitHoverInfoRT);
    }
    public void HideCharacterPortraitHoverInfo() {
        characterPortraitHoverInfoGO.SetActive(false);
    }
    public void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        PositionTooltip(Input.mousePosition, tooltipParent, rtToReposition, boundsRT);
    }
    // private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
    //     var v3 = position;
    //
    //     rtToReposition.pivot = new Vector2(0f, 1f);
    //     smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;
    //
    //     // if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Cross 
    //     //     || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Check 
    //     //     || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Link) {
    //     //     v3.x += 100f;
    //     //     v3.y -= 32f;
    //     // } else {
    //     //     v3.x += 25f;
    //     //     v3.y -= 25f;
    //     // }
    //     
    //     // tooltipParent.transform.position = v3;
    //     //
    //     // if (rtToReposition.sizeDelta.y >= Screen.height) {
    //     //     return;
    //     // }
    //
    //     Vector3 clampedPos = KeepFullyOnScreen(smallInfoBGRT, v3, canvasRectTransform);
    //     tooltipParent.transform.position = clampedPos;
    //
    //     // cornersOutside.Clear();
    //     // boundsRT.GetWorldCorners(cornerVectors);
    //     // for (int i = 0; i < 4; i++) {
    //     //     Vector3 localSpacePoint = mainRT.InverseTransformPoint(cornerVectors[i]);
    //     //     // If parent (canvas) does not contain checked items any point
    //     //     if (!mainRT.rect.Contains(localSpacePoint)) {
    //     //         cornersOutside.Add(i);
    //     //     }
    //     // }
    //
    //     // boundsRT.GetLocalCorners(cornerVectors);
    //     // for (int i = 0; i < cornersOutside.Count; i++) {
    //     //     int corner = cornersOutside[i];
    //     //     if (corner == 0) {
    //     //         //bottom left or bottom right corner is outside, adjust y position upwards
    //     //         Vector3 cornerWorldPos = cornerVectors[corner];
    //     //         Debug.Log($"Corner screen pos: {cornerWorldPos}.");
    //     //         //move position up by pixels outside + buffer
    //     //         RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
    //     //         Vector3 newPos = tooltipParentRT.anchoredPosition;
    //     //         newPos.y += Mathf.Abs(cornerWorldPos.y);
    //     //         tooltipParentRT.anchoredPosition = newPos;
    //     //     }
    //     // }
    //
    //     // if (cornersOutside.Count != 0) {
    //     //     if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
    //     //         if (cornersOutside.Contains(0)) {
    //     //             //bottom side and right side are outside, move anchor to bottom right
    //     //             rtToReposition.pivot = new Vector2(1f, 0f);
    //     //             smallInfoBGParentLG.childAlignment = TextAnchor.LowerRight;
    //     //         } else {
    //     //             //right side is outside, move anchor to top right side
    //     //             rtToReposition.pivot = new Vector2(1f, 1f);
    //     //             smallInfoBGParentLG.childAlignment = TextAnchor.UpperRight;
    //     //         }
    //     //     } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
    //     //         //bottom side is outside, move anchor to bottom left
    //     //         rtToReposition.pivot = new Vector2(0f, 0f);
    //     //         smallInfoBGParentLG.childAlignment = TextAnchor.LowerLeft;
    //     //     }
    //     //     rtToReposition.localPosition = Vector3.zero;
    //     // }
    // }
    private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        Vector3 v3 = position;
        
        if (tooltipParent.transform.parent != smallInfoCanvasRT) {
            tooltipParent.transform.SetParent(smallInfoCanvasRT);    
        }
        if (tooltipParent.transform.localScale != Vector3.one) {
            tooltipParent.transform.localScale = Vector3.one;    
        }

        rtToReposition.pivot = new Vector2(0f, 1f);
        RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
        tooltipParentRT.pivot = new Vector2(0f, 0f);

        UtilityScripts.Utilities.GetAnchorMinMax(TextAnchor.LowerLeft, out var anchorMin, out var anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;

        smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;

        if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Cross 
            || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Check 
            || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Link) {
            v3.x += 100f;
            v3.y -= 32f;
        } else {
            v3.x += 25f;
            v3.y -= 25f;
        }
        
        Vector3 clampedPos = KeepFullyOnScreen(smallInfoBGRT, v3, smallInfoCanvas, smallInfoCanvasRT);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(smallInfoCanvasRT, clampedPos, null, out var localPoint);
        
        (tooltipParent.transform as RectTransform).localPosition = localPoint; //clampedPos;
    }
     private Vector3 KeepFullyOnScreen(RectTransform rect, Vector3 newPos, Canvas canvas, RectTransform CanvasRect) {
         float minX = 0f;
         var scaleFactor = canvas.scaleFactor;
         float maxX = ((CanvasRect.sizeDelta.x * scaleFactor) - (rect.sizeDelta.x * scaleFactor));
         float minY = rect.sizeDelta.y * scaleFactor;
         float maxY = CanvasRect.sizeDelta.y * scaleFactor;
        
         newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
         newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        
         return newPos;
     }
    public void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt) {
        tooltipParent.transform.SetParent(position.transform);
        RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
        tooltipParentRT.pivot = position.pivot;

        UtilityScripts.Utilities.GetAnchorMinMax(position.anchor, out var anchorMin, out var anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;
        tooltipParentRT.anchoredPosition = Vector2.zero;

        smallInfoBGParentLG.childAlignment = position.anchor;
        rt.pivot = position.pivot;
    }
    public void ShowSmallLocationInfo(Region region, Vector3 pos, string subText = "") {
        locationSmallInfo.ShowRegionInfo(region, subText);
        locationSmallInfoRT.position = pos;
    }
    public void HideSmallLocationInfo() {
        locationSmallInfo.Hide();
    }
    private bool IsSmallLocationInfoShowing() {
        return locationSmallInfoRT.gameObject.activeSelf;
    }
    public Region GetCurrentlyShowingSmallInfoLocation() {
        if (IsSmallLocationInfoShowing()) {
            return locationSmallInfo.region;
        }
        return null;
    }
    public void ShowCharacterNameplateTooltip(Character character, UIHoverPosition position) {
        _characterNameplateTooltip.SetObject(character);
        _characterNameplateTooltip.gameObject.SetActive(true);
        _characterNameplateTooltip.SetPosition(position);
    }
    public void HideCharacterNameplateTooltip() {
        _characterNameplateTooltip.gameObject.SetActive(false);
    }
    public void ShowTileObjectNameplateTooltip(TileObject tileObject, UIHoverPosition position) {
        _tileObjectNameplateTooltip.SetObject(tileObject);
        _tileObjectNameplateTooltip.gameObject.SetActive(true);
        _tileObjectNameplateTooltip.SetPosition(position);
    }
    public void HideTileObjectNameplateTooltip() {
        _tileObjectNameplateTooltip.gameObject.SetActive(false);
    }
    public void ShowStructureNameplateTooltip(LocationStructure structure, UIHoverPosition position) {
        _structureNameplateTooltip.SetObject(structure);
        _structureNameplateTooltip.gameObject.SetActive(true);
        _structureNameplateTooltip.SetPosition(position);
    }
    public void HideStructureNameplateTooltip() {
        _structureNameplateTooltip.gameObject.SetActive(false);
    }
    #endregion

    #region Developer Notifications NPCSettlement
    private void ShowDeveloperNotification(string text, int expirationTicks, UnityAction onClickAction) {
        developerNotificationArea.ShowNotification(text, expirationTicks, onClickAction);
    }
    #endregion
    
    #region UI Utilities
    public List<PopupMenuBase> openedPopups { get; private set; }
    private void OnUIMenuOpened(InfoUIBase menu) {
        latestOpenedInfoUI = menu;
    }
    private void OnUIMenuClosed(InfoUIBase menu) {
        if (latestOpenedInfoUI == menu) {
            latestOpenedInfoUI = null;
        }
    }
    private void OnPopupMenuOpened(PopupMenuBase menu) {
        if (!openedPopups.Contains(menu)) {
            openedPopups.Add(menu);    
        }
        //latestOpenedPopup = menu;
    }
    private void OnPopupMenuClosed(PopupMenuBase menu) {
        // if(latestOpenedPopup == menu) {
        //     latestOpenedPopup = null;
        // }
        openedPopups.Remove(menu);
    }
    /// <summary>
    /// Checker for if the mouse is currently over a UI Object. 
    /// </summary>
    /// <returns>True or false.</returns>>
    public bool IsMouseOnUI() {
        if (_pointer != null) {
            _pointer.position = Input.mousePosition;
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(_pointer, _raycastResults);

            return _raycastResults.Count > 0 && _raycastResults.Any(
                go => go.gameObject.layer == LayerMask.NameToLayer("UI") || 
                      go.gameObject.layer == LayerMask.NameToLayer("WorldUI") || 
                      go.gameObject.CompareTag("Map_Click_Blocker"));    
        }
        return false;
    }
    public void OpenObjectUI(object obj) {
        if (obj is Character character) {
            ShowCharacterInfo(character, true);
        } else if (obj is NPCSettlement settlement) {
            ShowSettlementInfo(settlement);
            //if (settlement.allStructures.Count > 0) {
            //    ShowStructureInfo(settlement.allStructures.First());
            //}
            // ShowRegionInfo(settlement.region);
        } else if (obj is Faction faction) {
            ShowFactionInfo(faction);
        } else if (obj is Minion minion) {
            ShowCharacterInfo(minion.character, true);
        } else if (obj is Party party) {
            ShowPartyInfo(party);
        } else if (obj is TileObject tileObject) {
            ShowTileObjectInfo(tileObject);
        } else if (obj is LocationStructure structure) {
            if (!structure.hasBeenDestroyed) {
                ShowStructureInfo(structure);    
            }
            // structure.CenterOnStructure();
        }
    }
    public bool IsMouseOnMapObject() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        _raycastResults.Clear();
        //_raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, _raycastResults);

        if (_raycastResults.Count > 0) {
            foreach (var go in _raycastResults) {
                if (go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsMouseOnUIOrMapObject() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        _raycastResults.Clear();
        //List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, _raycastResults);

        if (_raycastResults.Count > 0) {
            foreach (var go in _raycastResults) {
                if (go.gameObject.layer == LayerMask.NameToLayer("UI") || go.gameObject.layer == LayerMask.NameToLayer("WorldUI") 
                    || go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    return true;
                }
            }
        }
        return false;
    }
    public int GetMouseOnUIOrMapObjectValue() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        _raycastResults.Clear();
        //List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, _raycastResults);

        if (_raycastResults.Count > 0) {
            foreach (var go in _raycastResults) {
                if (go.gameObject.layer == LayerMask.NameToLayer("UI") || go.gameObject.layer == LayerMask.NameToLayer("WorldUI")) {
                    return 0;
                } else if (go.gameObject.CompareTag("Character Marker")) {
                    return 1;
                } else if (go.gameObject.CompareTag("Map Object")) {
                    return 2;
                }
            }
        }
        return -1;
    }
    public void SetCoverState(bool state, bool blockClicks = true) {
        cover.SetActive(state);
        cover.GetComponent<Image>().raycastTarget = blockClicks;
    }
    private void OnInteractionMenuOpened() {
       if (characterInfoUI.isShowing) {
            _lastOpenedInfoUI = characterInfoUI;
       }
       if (characterInfoUI.isShowing) {
            characterInfoUI.gameObject.SetActive(false);
       }
    }
    private void OnInteractionMenuClosed() {
        //reopen last opened menu
        if (_lastOpenedInfoUI != null) {
            _lastOpenedInfoUI.OpenMenu();
            _lastOpenedInfoUI = null;
        }
    }
    public void SetTempDisableShowInfoUI(bool state) {
        tempDisableShowInfoUI = state;
    }
    public Character GetCurrentlySelectedCharacter() {
        if (characterInfoUI.isShowing) {
            return characterInfoUI.activeCharacter;
        } else if (monsterInfoUI.isShowing) {
            return monsterInfoUI.activeMonster;
        }
        return null;
    }
    public IPointOfInterest GetCurrentlySelectedPOI() {
        if (characterInfoUI.isShowing) {
            return characterInfoUI.activeCharacter;
        } else if (monsterInfoUI.isShowing) {
            return monsterInfoUI.activeMonster;
        } else if (tileObjectInfoUI.isShowing) {
            return tileObjectInfoUI.activeTileObject;
        }
        return null;
    }
    public object GetCurrentlySelectedObject() {
        IPointOfInterest poi = GetCurrentlySelectedPOI();
        if(poi == null) {
            if (settlementInfoUI.isShowing) {
                return settlementInfoUI.activeSettlement;
            } else if (structureInfoUI.isShowing) {
                return structureInfoUI.activeStructure;
            } else if (structureRoomInfoUI.isShowing) {
                return structureRoomInfoUI.activeRoom;
            } else if (partyInfoUI.isShowing) {
                return partyInfoUI.activeParty;
            }
        }
        return null;
    }
    #endregion

    #region Object Pooling
    /*
     * Use this to instantiate UI Objects, so that the program can normalize it's
     * font sizes.
     * */
    internal GameObject InstantiateUIObject(string prefabObjName, Transform parent) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
        return go;
    }
    #endregion

    //#region Nameplate
    //public LandmarkNameplate CreateLandmarkNameplate(BaseLandmark landmark) {
    //    GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("LandmarkNameplate", worldUIParent);
    //    nameplateGO.transform.localScale = Vector3.one;
    //    LandmarkNameplate nameplate = nameplateGO.GetComponent<LandmarkNameplate>();
    //    nameplate.SetLandmark(landmark);
    //    return nameplate;
    //}
    //#endregion

    #region Object Picker
    public void ShowClickableObjectPicker<T>(List<T> choices, Action<object> onClickAction, IComparer<T> comparer = null
        , Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverAction = null, Action<T> onHoverExitAction = null, 
        string identifier = "", bool showCover = false, int layer = 9, bool closable = true, Func<string,Sprite> portraitGetter = null, bool shouldShowConfirmationWindowOnPick = false, bool asButton = false) {

        objectPicker.ShowClickable(choices, onClickAction, comparer, validityChecker, title, onHoverAction,
            onHoverExitAction, identifier, showCover, layer, portraitGetter, asButton, shouldShowConfirmationWindowOnPick);
        Messenger.Broadcast(UISignals.OBJECT_PICKER_SHOWN, identifier);
        //Pause();
        //SetSpeedTogglesState(false);
    }
    //public void ShowDraggableObjectPicker<T>(List<T> choices, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "") {
    //    objectPicker.ShowDraggable(choices, comparer, validityChecker, title);
    //}
    public void HideObjectPicker() {
        objectPicker.Close();
        //Unpause();
        //SetSpeedTogglesState(true);
    }
    public bool IsObjectPickerOpen() {
        return objectPicker.gameObject.activeSelf;
    }
    #endregion

    #region For Testing
    public void SetUIState(bool state) {
        gameObject.SetActive(state);
        Messenger.Broadcast(UISignals.UI_STATE_SET);
    }
    public void DateHover() {
        ShowSmallInfo($"Day: {GameManager.Instance.continuousDays.ToString()} Tick: {GameManager.Instance.Today().tick.ToString()}");
    }
    [ExecuteInEditMode]
    [ContextMenu("Set All Scroll Rect Scroll Speed")]
    public void SetAllScrollSpeed() {
        ScrollRect[] allScroll = gameObject.GetComponentsInChildren<ScrollRect>(true);
        for (int i = 0; i < allScroll.Length; i++) {
            ScrollRect rect = allScroll[i];
            rect.scrollSensitivity = 25f;
        }
    }
    #endregion

    #region NPCSettlement Info
    public Sprite GetAreaCenterSprite(string name) {
        for (int i = 0; i < areaCenterSprites.Length; i++) {
            if (areaCenterSprites[i].name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
                return areaCenterSprites[i];
            }
        }
        return null;
    }
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField] internal FactionInfoUI factionInfoUI;
    public void ShowFactionInfo(Faction faction) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        //factionInfoUI.SetData(faction);
        //factionInfoUI.OpenMenu();
        FactionInfoHubUI.Instance.ShowFaction(faction);
    }
    //public void UpdateFactionInfo() {
    //    if (factionInfoUI.isShowing) {
    //        factionInfoUI.UpdateFactionInfo();
    //    }
    //}
    #endregion

    #region Character Info
    [Space(10)]
    [Header("Character Info")]
    [SerializeField] internal CharacterInfoUI characterInfoUI;
    public void ShowCharacterInfo(Character character, bool centerOnCharacter = false) {
        // if (GameManager.Instance.gameHasStarted == false) {
        //     return;
        // }
        Character characterToShow = character;
        if(character.isLycanthrope) {
            characterToShow = character.lycanData.activeForm;
        }
        if (characterToShow == null || !characterToShow.hasMarker) {
            //Do not show characters that have no body anymore
            return;
        }
        if(characterToShow.isNormalCharacter) {
            if (tempDisableShowInfoUI) {
                SetTempDisableShowInfoUI(false);
                return;
            }
            characterInfoUI.SetData(characterToShow);
            characterInfoUI.OpenMenu();
            if (centerOnCharacter) {
                characterToShow.CenterOnCharacter();
            }
        } else {
            ShowMonsterInfo(characterToShow, centerOnCharacter);
        }
    }
    public void UpdateCharacterInfo() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.UpdateCharacterInfo();
        }
    }
    //private void OnPartyStartedTravelling(Party party) {
    //    if(characterInfoUI.isShowing && party.characters.Contains(characterInfoUI.activeCharacter)) {
    //        characterInfoUI.activeCharacter.CenterOnCharacter();
    //    }
    //}
    //private void OnPartyDoneTravelling(Party party) {
    //    if (characterInfoUI.isShowing && party.characters.Contains(characterInfoUI.activeCharacter)) {
    //        characterInfoUI.activeCharacter.CenterOnCharacter();
    //    }
    //}
    public void OnCameraOutOfFocus() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.OnClickCloseMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.OnClickCloseMenu();
        }
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.OnClickCloseMenu();
        }
        if (structureInfoUI.isShowing) {
            structureInfoUI.OnClickCloseMenu();
        }
        if (settlementInfoUI.isShowing) {
            settlementInfoUI.OnClickCloseMenu();
        }
    }
    #endregion

    #region Minion Info
    [FormerlySerializedAs("minionInfoUI")]
    [Space(10)]
    [Header("Monster Info")]
    [SerializeField] internal MonsterInfoUI monsterInfoUI;
    private void ShowMonsterInfo(Character character, bool centerOnCharacter = false) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        monsterInfoUI.SetData(character);
        monsterInfoUI.OpenMenu();
        if (centerOnCharacter) {
            character.CenterOnCharacter();
        }
    }
    private void UpdateMonsterInfo() {
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.UpdateMonsterInfo();
        }
    }
    #endregion

    #region Tile Object Info
    [Space(10)]
    [Header("Tile Object Info")]
    [SerializeField] internal TileObjectInfoUI tileObjectInfoUI;
    public void ShowTileObjectInfo(TileObject tileObject) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        tileObjectInfoUI.SetData(tileObject);
        tileObjectInfoUI.OpenMenu();
    }
    public void UpdateTileObjectInfo() {
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.UpdateTileObjectInfo();
        }
    }
    #endregion

    #region Party Info
    [Space(10)]
    [Header("Party Info")]
    [SerializeField] internal PartyInfoUI partyInfoUI;
    public void ShowPartyInfo(Party party) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        partyInfoUI.SetData(party);
        partyInfoUI.OpenMenu();
    }
    public void UpdatePartyInfo() {
        if (partyInfoUI.isShowing) {
            partyInfoUI.UpdatePartyInfo();
        }
    }
    #endregion

    #region Structure Info
    [Space(10)]
    [Header("Structure Info")]
    [SerializeField] public StructureInfoUI structureInfoUI;
    public void ShowStructureInfo(LocationStructure structure, bool centerOnStructure = true) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        // if (structure.structureType == STRUCTURE_TYPE.SPIRE) {
        //     onSpireClicked?.Invoke();
        // }
        // if (structure.structureType == STRUCTURE_TYPE.MARAUD) {
        //     onMaraudClicked?.Invoke(structure);
        // }
        // if (structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
        //     onTortureChamberClicked?.Invoke(structure);
        // }
        // if (structure.structureType == STRUCTURE_TYPE.KENNEL) {
        //     onKennelClicked?.Invoke(structure);
        // }
        // if (structure.structureType == STRUCTURE_TYPE.DEFENSE_POINT) {
        //     onDefensePointClicked?.Invoke(structure);
        // }
        structureInfoUI.SetData(structure);
        structureInfoUI.OpenMenu();
        if (centerOnStructure) {
            structure.CenterOnStructure();
        }
    }
    private void UpdateStructureInfo() {
        if (structureInfoUI.isShowing) {
            structureInfoUI.UpdateStructureInfoUI();
        }
    }
    private void OnStructureDestroyed(LocationStructure structure) {
        CheckStructureInfoForClosure(structure);
    }
    private void CheckStructureInfoForClosure(LocationStructure structure) {
        if(structureInfoUI.isShowing && structureInfoUI.activeStructure == structure) {
            structureInfoUI.CloseMenu();
        }
    }
    #endregion
    
    #region Unbuilt Structure Info
    [Space(10)]
    [Header("Unbuilt Structure Info")]
    [SerializeField] public UnbuiltStructureInfoUI unbuiltStructureInfoUI;
    public void ShowUnbuiltStructureInfo(LocationStructureObject p_structureObject) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        unbuiltStructureInfoUI.SetData(p_structureObject);
        unbuiltStructureInfoUI.OpenMenu();
    }
    private void UpdateUnbuiltStructureInfo() {
        if (unbuiltStructureInfoUI.isShowing) {
            unbuiltStructureInfoUI.UpdateUnbuiltStructureInfoUI();
        }
    }
    #endregion

    #region Structure Room Info
    [Space(10)]
    [Header("Structure Room Info")]
    public StructureRoomInfoUI structureRoomInfoUI;
    public void ShowStructureRoomInfo(StructureRoom room) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        structureRoomInfoUI.SetData(room);
        structureRoomInfoUI.OpenMenu();
    }
    public void UpdateStructureRoomInfo() {
        if (structureRoomInfoUI.isShowing) {
            structureRoomInfoUI.UpdateInfo();
        }
    }
    #endregion

    #region Settlement Info
    [Space(10)]
    [Header("Settlement Info")]
    [SerializeField] public SettlementInfoUI settlementInfoUI;
    public void ShowSettlementInfo(BaseSettlement settlement) {
        //Only show settlement info on village type settlements
        //If settlement is a dungeon, etc., show structure info instead
        if (settlement.locationType != LOCATION_TYPE.VILLAGE) {
            if (settlement.allStructures.Count > 0) {
                ShowStructureInfo(settlement.allStructures.First());
            }
            return;
        }

        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        settlementInfoUI.SetData(settlement);
        settlementInfoUI.OpenMenu();
    }
    public void UpdateSettlementInfo() {
        if (settlementInfoUI.isShowing) {
            settlementInfoUI.UpdateSettlementInfoUI();
        }
    }
    #endregion

    #region Console
    [Space(10)]
    [Header("Console")]
    [SerializeField] internal ConsoleBase consoleUI;
    public bool IsConsoleShowing() {
        //return false;
        return consoleUI.isShowing;
    }
    public void ToggleConsole() {
        if (consoleUI.isShowing) {
            HideConsole();
        } else {
            ShowConsole();
        }
    }
    public void ShowConsole() {
        consoleUI.ShowConsole();
    }
    public void HideConsole() {
        consoleUI.HideConsole();
    }
    #endregion

    #region Save
    public void Save() {
        //Save savefile = new Save();
        //savefile.hextiles = new List<HextileSave>();
        //for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
        //    if(GridMap.Instance.hexTiles[i].landmarkOnTile != null) {
        //        HextileSave hextileSave = new HextileSave();
        //        hextileSave.SaveTile(GridMap.Instance.hexTiles[i]);
        //        savefile.hextiles.Add(hextileSave);
        //    }
        //}
        //SaveGame.Save<Save>("SavedFile1", savefile);
        //LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
    #endregion

    #region Conversation Menu
    [Header("Conversation Menu")]
    [SerializeField] private ConversationMenu conversationMenu;
    public void OpenConversationMenu(List<ConversationData> conversationList, string titleText) {
        conversationMenu.Open(conversationList, titleText);
    }
    public bool IsConversationMenuOpen() {
        return conversationMenu.gameObject.activeSelf;
    }
    public void CloseConversationMenu() {
        conversationMenu.Close();
    }
    private void OnOpenConversationMenu() {
        SetCoverState(true);
        //playerNotificationParent.SetSiblingIndex(1);
    }
    private void OnCloseShareIntelMenu() {
        SetCoverState(false);
        //Unpause();
        //SetSpeedTogglesState(true);
        //playerNotificationParent.SetAsLastSibling();
    }
    #endregion

    #region Intel Notification
    [Header("Intel Notification")]
    [SerializeField] private GameObject intelPrefab;
    [SerializeField] private GameObject defaultNotificationPrefab;
    [SerializeField] private UIHoverPosition notificationHoverPos;
    [SerializeField] private Vector2 notificationHoverPosDefaultPosition;
    [SerializeField] private Vector2 notificationHoverPosModifiedPosition;
    [SerializeField] private GameObject searchFieldsParent;
    [SerializeField] private TMP_InputField notificationSearchField;
    [SerializeField] private GameObject searchFieldClearBtn;
    [SerializeField] private GameObject filtersGO;
    [SerializeField] private LogFilterItem[] allFilters;
    [SerializeField] private Toggle showAllToggle;
    [SerializeField] private int maxPlayerNotif;
    private List<LOG_TAG> notificationFilters;
    public ScrollRect playerNotifScrollRect;
    public List<PlayerNotificationItem> activeNotifications = new List<PlayerNotificationItem>(); //notifications that are currently being shown.
    private List<string> activeNotificationIDs = new List<string>();
    
    private void ShowPlayerNotification(IIntel intel) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        IntelNotificationItem newItem = newIntelGO.GetComponent<IntelNotificationItem>();
        newItem.Initialize(intel, OnNotificationDestroyed);
        newItem.SetHoverPosition(notificationHoverPos);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem, intel.log);
    }
    private void ShowPlayerNotification(Log log) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, OnNotificationDestroyed);
        newItem.SetHoverPosition(notificationHoverPos);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem, log);        
    }
    public void ShowPlayerNotification(in Log log, int tick) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, tick, OnNotificationDestroyed);
        newItem.SetHoverPosition(notificationHoverPos);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem, log);
    }
    public void ShowPlayerNotification(IIntel intel, in Log log, int tick) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        IntelNotificationItem newItem = newIntelGO.GetComponent<IntelNotificationItem>();
        newItem.Initialize(intel, OnNotificationDestroyed);
        newItem.SetHoverPosition(notificationHoverPos);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem, log);
    }
    private void PlaceNewNotification(PlayerNotificationItem newNotif, Log shownLog) {
        //NOTE: Removed this since this was only needed when we would also show notifications of an action that the character is currently doing
        //but since we now only show logs after the fact, then this would be irrelevant and could cause notifications to get overwritten
        //example is Vampiric embrace that has a success log ([Actor Name] gave [Target Name] a Vampiric Embrace!) which is also an intel,
        //but also has a result log. ([Target name] contracted Vampirism from [Actor name]). But the result notification will overwrite the
        //intel notification since they both came from the same action
        
        //check if the log used is from a GoapAction
        //then check all other currently showing notifications, if it is from the same goap action
        //replace that log with this new one
        // PlayerNotificationItem itemToReplace = null;
        // if (shownLog.hasValue && !string.IsNullOrEmpty(shownLog.actionID)) {
        //     for (int i = 0; i < activeNotifications.Count; i++) {
        //         PlayerNotificationItem currItem = activeNotifications[i];
        //         if (!string.IsNullOrEmpty(currItem.fromActionID) && shownLog.actionID == currItem.fromActionID) {
        //             itemToReplace = currItem;
        //             break;
        //         }
        //     }
        // }
        // if (itemToReplace != null) {
        //     itemToReplace.DeleteNotification();
        // }
        activeNotifications.Add(newNotif);
        activeNotificationIDs.Add(shownLog.persistentID);
        if (activeNotifications.Count > maxPlayerNotif) {
            activeNotifications[0].DeleteOldestNotification();
        }
        UpdateSearchFieldsState();
        if (HasSearchCriteria()) {
            List<string> filteredLogIDs = DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, notificationSearchField.text, notificationFilters);
            if (filteredLogIDs.Contains(newNotif.logPersistentID)) {
                newNotif.DoTweenHeight();
                newNotif.TweenIn();    
            } else {
                newNotif.QueueAdjustHeightOnEnable();
            }
            FilterNotifications(filteredLogIDs);
        } else {
            newNotif.DoTweenHeight();
            newNotif.TweenIn();    
        }
    }
    private void OnNotificationDestroyed(PlayerNotificationItem item) {
        activeNotifications.Remove(item);
        activeNotificationIDs.Remove(item.logPersistentID);
        UpdateSearchFieldsState();
    }
    private void FilterNotifications(List<string> filteredLogIDs = null) {
        if (filteredLogIDs == null) {
            filteredLogIDs = DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, notificationSearchField.text, notificationFilters);
        }
        for (int i = 0; i < activeNotifications.Count; i++) {
            PlayerNotificationItem item = activeNotifications[i];
            if (filteredLogIDs != null && filteredLogIDs.Contains(item.logPersistentID)) {
                item.gameObject.SetActive(true);
                item.transform.SetSiblingIndex(i);
            } else {
                item.gameObject.SetActive(false);
            }
        }
    }
    private bool HasSearchCriteria() {
        return !string.IsNullOrEmpty(notificationSearchField.text) || (notificationFilters.Count > 0 && notificationFilters.Count < DatabaseManager.Instance.mainSQLDatabase.allLogTags.Count);
    }
    private void UpdateSearchFieldsState() {
        searchFieldsParent.gameObject.SetActive(activeNotificationIDs.Count > 0);
    }
    private void OnEndNotificationSearchEdit(string text) {
        searchFieldClearBtn.gameObject.SetActive(!string.IsNullOrEmpty(text)); //show clear button if there is a given text
        FilterNotifications();
    }
    public void ToggleFilters() {
        filtersGO.gameObject.SetActive(!filtersGO.activeSelf);
    }
    private void OnToggleFilter(bool isOn, LOG_TAG tag) {
        if (isOn) {
            notificationFilters.Add(tag);
        } else {
            notificationFilters.Remove(tag);
        }
        showAllToggle.SetIsOnWithoutNotify(AreAllFiltersOn());
        FilterNotifications();
    }
    private bool AreAllFiltersOn() {
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            if (!filterItem.isOn) {
                return false;
            }
        }
        return true;
    }
    public void OnToggleAllFilters(bool state) {
        notificationFilters.Clear();
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            filterItem.SetIsOnWithoutNotify(state);
            if (state) {
                //if search all is enabled then add filter. If it is not do not do anything to the list since list was cleared beforehand.
                notificationFilters.Add(filterItem.filterType);    
            }
        }
        FilterNotifications();
    }
    #endregion

    #region Yes/No
    [Header("Yes or No Confirmation")]
    public YesNoConfirmation yesNoConfirmation;
    private bool _yesNoPauseAndResume;
    /// <summary>
    /// Show a yes/no pop up window
    /// </summary>
    /// <param name="header">The title of the window.</param>
    /// <param name="question">The question answerable by yes/no.</param>
    /// <param name="onClickYesAction">The action to perform once the user clicks yes. NOTE: Closing of this window is added by default</param>
    /// <param name="onClickNoAction">The action to perform once the user clicks no. NOTE: Closing of this window is added by default</param>
    /// <param name="showCover">Should this popup also show a cover that covers the game.</param>
    /// <param name="layer">The sorting layer order of this window.</param>
    /// <param name="yesBtnText">The yes button text.</param>
    /// <param name="noBtnText">The no button text.</param>
    /// <param name="yesBtnInteractable">Should the yes button be clickable?</param>
    /// <param name="noBtnInteractable">Should the no button be clickable?</param>
    /// <param name="pauseAndResume">Should the game pause when this window shows, and resume when it closes?</param>
    /// <param name="yesBtnActive">Should the yes button be visible?</param>
    /// <param name="noBtnActive">Should the no button be visible?</param>
    /// <param name="yesBtnInactiveHoverAction">Action to execute when user hover over an un-clickable yes button</param>
    /// <param name="yesBtnInactiveHoverExitAction">Action to execute when user hover over an un-clickable no button</param>
    /// <param name="onClickCloseAction">Action to execute when clicking on close btn. NOTE: Hide action is added by default</param>
    /// <param name="onHideUIAction">Action to execute when popup is closed.</param>
    public void ShowYesNoConfirmation(string header, string question, Action onClickYesAction = null, Action onClickNoAction = null,
        bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, bool pauseAndResume = false, 
        bool yesBtnActive = true, bool noBtnActive = true, Action yesBtnInactiveHoverAction = null, Action yesBtnInactiveHoverExitAction = null, System.Action onClickCloseAction = null,
        System.Action onHideUIAction = null) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowYesNoConfirmation(header, question, onClickYesAction, onClickNoAction, 
                showCover, layer, yesBtnText, noBtnText, yesBtnInteractable, noBtnInteractable, pauseAndResume,
                yesBtnActive, noBtnActive, yesBtnInactiveHoverAction, yesBtnInactiveHoverExitAction));
            return;
        }
        _yesNoPauseAndResume = pauseAndResume;
        if (_yesNoPauseAndResume && !IsObjectPickerOpen()) {
            //if object picker is already being shown, do not pause, so that this does not mess with the previously set speed. 
            Pause();
            SetSpeedTogglesState(false);    
        }
        yesNoConfirmation.ShowYesNoConfirmation(header, question, onClickYesAction, onClickNoAction, showCover, layer, yesBtnText, noBtnText, yesBtnInteractable, noBtnInteractable, 
            yesBtnActive, noBtnActive, yesBtnInactiveHoverAction, yesBtnInactiveHoverExitAction, onClickCloseAction, onHideUIAction);
    }
    public void HideYesNoConfirmation() {
        yesNoConfirmation.Close();
        if (!PlayerUI.Instance.TryShowPendingUI() && !IsObjectPickerOpen() && !optionsMenu.isShowing && _yesNoPauseAndResume) {
            ResumeLastProgressionSpeed(); //if no other UI was shown and object picker is not open, unpause game
        }
    }
    private void TweenIn(CanvasGroup canvasGroup) {
        canvasGroup.alpha = 0;
        RectTransform rectTransform = canvasGroup.transform as RectTransform; 
        rectTransform.anchoredPosition = new Vector2(0f, -30f);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, 0.5f)
            .SetEase(Ease.InSine));
        sequence.PrependInterval(0.2f);
        sequence.Play();
    }
    #endregion

    #region Trigger Flaw
    [Header("Trigger Flaw Confirmation")]
    public GameObject triggerFlawGO;
    [SerializeField] private CanvasGroup triggerFlawCanvasGroup;
    [SerializeField] private GameObject triggerFlawCover;
    [SerializeField] private TextMeshProUGUI triggerFlawDescriptionLbl;
    [SerializeField] private TextMeshProUGUI triggerFlawEffectLbl;
    [SerializeField] private TextMeshProUGUI triggerFlawManaCostLbl;
    [SerializeField] private Button triggerFlawYesBtn;
    [SerializeField] private Button triggerFlawNoBtn;
    [SerializeField] private Button triggerFlawCloseBtn;
    public void ShowTriggerFlawConfirmation(string question, string effect, string manaCost, Action onClickYesAction = null, bool showCover = false, int layer = 21, bool pauseAndResume = false) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowTriggerFlawConfirmation(question, effect, manaCost, onClickYesAction, showCover, layer, pauseAndResume));
            return;
        }
        if (pauseAndResume && !IsObjectPickerOpen()) {
            //if object picker is already being shown, do not pause, so that this does not mess with the previously set speed. 
            Pause();
            SetSpeedTogglesState(false);    
        }
        HidePlayerActionContextMenu();
        
        // if (pauseAndResume) {
        //     SetSpeedTogglesState(false);
        //     Pause();
        // }
        triggerFlawDescriptionLbl.text = question;
        triggerFlawEffectLbl.text = effect;
        triggerFlawManaCostLbl.text = manaCost;

        //clear all listeners
        triggerFlawYesBtn.onClick.RemoveAllListeners();
        triggerFlawNoBtn.onClick.RemoveAllListeners();
        triggerFlawCloseBtn.onClick.RemoveAllListeners();

        //hide confirmation menu on click
        triggerFlawYesBtn.onClick.AddListener(HideTriggerFlawConfirmation);
        triggerFlawNoBtn.onClick.AddListener(HideTriggerFlawConfirmation);
        triggerFlawCloseBtn.onClick.AddListener(HideTriggerFlawConfirmation);
        //specific actions
        if (onClickYesAction != null) {
            triggerFlawYesBtn.onClick.AddListener(onClickYesAction.Invoke);
        }

        triggerFlawGO.SetActive(true);
        triggerFlawGO.transform.SetSiblingIndex(layer);
        triggerFlawCover.SetActive(showCover);
        TweenIn(triggerFlawCanvasGroup);
    }
    private void HideTriggerFlawConfirmation() {
        triggerFlawGO.SetActive(false);
        if (!PlayerUI.Instance.TryShowPendingUI() && !IsObjectPickerOpen()) {
            ResumeLastProgressionSpeed(); //if no other UI was shown and object picker is not open, unpause game
        }
    }
    #endregion

    #region Important Notifications
    [Header("Important Notification")]
    [SerializeField] private ScrollRect importantNotifScrollView;
    [SerializeField] private GameObject importantNotifPrefab;
    public void ShowImportantNotification(GameDate date, string message, Action onClickAction) {
        if (GameManager.Instance.gameHasStarted == false) {
            return;
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(importantNotifPrefab.name, Vector3.zero, Quaternion.identity, importantNotifScrollView.content);
        ImportantNotificationItem item = go.GetComponent<ImportantNotificationItem>();
        item.Initialize(date, message, onClickAction);
    }
    #endregion

    #region Minion Card Info
    [Space(10)]
    [Header("Minion Card Info")]
    [SerializeField] private MinionCard minionCardTooltip;
    [SerializeField] private RectTransform minionCardRT;
    public void ShowMinionCardTooltip(Minion minion, UIHoverPosition position = null) {
        if (minionCardTooltip.minion != minion) {
            minionCardTooltip.SetMinion(minion);
        }
        if (!minionCardTooltip.gameObject.activeSelf) {
            minionCardTooltip.gameObject.SetActive(true);
        }
        if (position != null) {
            PositionMinionCardTooltip(position);
        } else {
            PositionMinionCardTooltip(Input.mousePosition);
        }
    }
    public void ShowMinionCardTooltip(UnsummonedMinionData minion, UIHoverPosition position = null) {
        if (!minionCardTooltip.minionData.Equals(minion)) {
            minionCardTooltip.SetMinion(minion);
        }
        if (!minionCardTooltip.gameObject.activeSelf) {
            minionCardTooltip.gameObject.SetActive(true);
        }
        if (position != null) {
            PositionMinionCardTooltip(position);
        } else {
            PositionMinionCardTooltip(Input.mousePosition);
        }
    }
    public void HideMinionCardTooltip() {
        minionCardTooltip.gameObject.SetActive(false);
    }
    private void PositionMinionCardTooltip(Vector3 screenPos) {
        minionCardTooltip.transform.SetParent(transform);
        var v3 = screenPos;

        minionCardRT.pivot = new Vector2(1f, 1f);

        //if (CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Cross || CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Check) {
        //    v3.x += 100f;
        //    v3.y -= 32f;
        //} else {
        //    v3.x += 25f;
        //    v3.y -= 25f;
        //}

        minionCardRT.transform.position = v3;

        //Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        //List<int> cornersOutside = new List<int>();
        //boundsRT.GetWorldCorners(corners);
        //for (int i = 0; i < 4; i++) {
        //    // Backtransform to parent space
        //    Vector3 localSpacePoint = mainRT.InverseTransformPoint(corners[i]);
        //    // If parent (canvas) does not contain checked items any point
        //    if (!mainRT.rect.Contains(localSpacePoint)) {
        //        cornersOutside.Add(i);
        //    }
        //}

        //if (cornersOutside.Count != 0) {
        //    string log = "Corners outside are: ";
        //    for (int i = 0; i < cornersOutside.Count; i++) {
        //        log += cornersOutside[i].ToString() + ", ";
        //    }
        //    //Debug.Log(log);
        //    if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
        //        if (cornersOutside.Contains(0)) {
        //            //bottom side and right side are outside, move anchor to bottom right
        //            rtToReposition.pivot = new Vector2(1f, 0f);
        //            smallInfoBGParentLG.childAlignment = TextAnchor.LowerRight;
        //        } else {
        //            //right side is outside, move anchor to top right side
        //            rtToReposition.pivot = new Vector2(1f, 1f);
        //            smallInfoBGParentLG.childAlignment = TextAnchor.UpperRight;
        //        }
        //    } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
        //        //bottom side is outside, move anchor to bottom left
        //        rtToReposition.pivot = new Vector2(0f, 0f);
        //        smallInfoBGParentLG.childAlignment = TextAnchor.LowerLeft;
        //    }
        //    rtToReposition.localPosition = Vector3.zero;
        //}
    }
    private void PositionMinionCardTooltip(UIHoverPosition position) {
        minionCardTooltip.transform.SetParent(position.transform);
        RectTransform tooltipParentRT = minionCardTooltip.transform as RectTransform;
        tooltipParentRT.pivot = position.pivot;

        UtilityScripts.Utilities.GetAnchorMinMax(position.anchor, out var anchorMin, out var anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;
        tooltipParentRT.anchoredPosition = Vector2.zero;
    }
    #endregion

    #region General Confirmation
    [Header("General Confirmation")]
    public GeneralConfirmationWithVisual generalConfirmationWithVisual;
    #endregion

    #region Popup Screens
    [FormerlySerializedAs("_demoUI")]
    [Header("Popup Screens")]
    [SerializeField] private PopUpScreensUI popUpScreensUI;
    public void ShowStartScenario(string message) {
        popUpScreensUI.ShowStartScreen(message);
    }
    public void ShowEndDemoScreen(string summary) {
        popUpScreensUI.ShowSummaryThenEndScreen(summary);
    }
    public bool IsShowingEndScreen() {
        return popUpScreensUI.IsShowingEndScreen();
    }
    #endregion

    #region Initial World Setup
    public InitialWorldSetupMenu initialWorldSetupMenu;
    #endregion

    #region Bottom Notification
    public BottomNotification bottomNotification;
    #endregion

    #region Logs
    public Sprite GetLogTagSprite(LOG_TAG tag) {
        if (logTagSpriteDictionary.ContainsKey(tag)) {
            return logTagSpriteDictionary[tag];
        }
        throw new Exception($"No Log tag sprite for tag {tag.ToString()}");
    }
    #endregion

    #region Overlap UI
    public void InitializeOverlapUI() {
        for (int i = 0; i < unallowOverlaps.Count; i++) {
            UnallowOverlaps currOverlap = unallowOverlaps[i];
            currOverlap.Initialize();
        }
    }
    public void AddUnallowOverlapUI(UnallowOverlaps overlap) {
        unallowOverlaps.Add(overlap);
    }
    public bool DoesUIOverlap(UnallowOverlaps overlap) {
        return GetOverlappedUI(overlap) != null;
    }
    public UnallowOverlaps GetOverlappedUI(UnallowOverlaps overlap) {
        for (int i = 0; i < unallowOverlaps.Count; i++) {
            UnallowOverlaps currOverlap = unallowOverlaps[i];
            if (currOverlap != overlap && currOverlap.gameObject.activeInHierarchy) {
                if (currOverlap.rectTransform.RectOverlaps(overlap.rectTransform)) {
                    return currOverlap;
                }
            }
        }
        return null;
    }
    #endregion

    #region Biolab UI
    [FormerlySerializedAs("_biolabUIController")] [Header("Biolab")] 
    public BiolabUIController biolabUIController;
    public void ShowBiolabUI() {
        Pause();
        SetSpeedTogglesState(false);
        biolabUIController.Open();
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        InputManager.Instance.SetSpecificHotkeyEnabledState(KeyCode.Escape, true);
    }
    private void OnCloseBiolabUI() {
        SetSpeedTogglesState(true);
        ResumeLastProgressionSpeed();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
    }
    #endregion

    #region Scheme UI
    [Header("Scheme")]
    [SerializeField] private SchemeUIController _schemeUIController;
    public void ShowSchemeUI(Character p_targetCharacter, object p_otherTarget, SchemeData p_schemeUsed) {
        Pause();
        SetSpeedTogglesState(false);
        _schemeUIController.Show(p_targetCharacter, p_otherTarget, p_schemeUsed, OnCloseSchemeUI);
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        Messenger.Broadcast(UISignals.SCHEME_UI_SHOWN);
    }
    private void OnCloseSchemeUI() {
        SetSpeedTogglesState(true);
        ResumeLastProgressionSpeed();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
    }
    #endregion

    #region Context Menu
    [Header("Context Menu")]
    public ContextMenuUIController contextMenuUIController;
    private bool _allowContextMenuInteractions = true;
    public void ShowPlayerActionContextMenu(IPlayerActionTarget p_target, Vector3 p_followTarget, bool p_isScreenPosition) {
        if (!_allowContextMenuInteractions) { return; }
        PlayerManager.Instance.player.SetCurrentPlayerActionTarget(p_target);
        List<IContextMenuItem> contextMenuItems = RuinarchListPool<IContextMenuItem>.Claim();
        PopulatePlayerActionContextMenuItems(contextMenuItems, p_target);
        contextMenuUIController.SetFollowPosition(p_followTarget, p_isScreenPosition);
        contextMenuUIController.ShowContextMenu(contextMenuItems, Input.mousePosition, p_target.name, InputManager.Instance.currentCursorType);
        Messenger.Broadcast(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, p_target);
        RuinarchListPool<IContextMenuItem>.Release(contextMenuItems);
    }
    public void RefreshPlayerActionContextMenuWithNewTarget(IPlayerActionTarget p_target) {
        if (!_allowContextMenuInteractions) { return; }
        PlayerManager.Instance.player.SetCurrentPlayerActionTarget(p_target);
        List<IContextMenuItem> contextMenuItems = RuinarchListPool<IContextMenuItem>.Claim();
        PopulatePlayerActionContextMenuItems(contextMenuItems, p_target);
        //List<IContextMenuItem> contextMenuItems = GetPlayerActionContextMenuItems(p_target);
        contextMenuUIController.ShowContextMenu(contextMenuItems, p_target.name);
        Messenger.Broadcast(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, p_target);
        RuinarchListPool<IContextMenuItem>.Release(contextMenuItems);
    }
    public void DisableContextMenuInteractions() {
        _allowContextMenuInteractions = false;
    }
    public void EnableContextMenuInteractions() {
        _allowContextMenuInteractions = true;
    }
    public void HidePlayerActionContextMenu() {
        PlayerManager.Instance.player.SetCurrentPlayerActionTarget(null);
        contextMenuUIController.HideUI();
    }
    public bool IsContextMenuShowing() {
        return contextMenuUIController.IsShowing();
    }
    public bool IsContextMenuShowingForTarget(IPlayerActionTarget p_target) {
        return IsContextMenuShowing() && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target;
    }
    private void OnHoverOverPlayerActionContextMenuItem(IContextMenuItem p_item, UIHoverPosition p_hoverPosition) {
        if (p_item is PlayerAction playerAction) {
            OnHoverPlayerAction(playerAction, p_hoverPosition, PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
        } else if (p_item is Trait trait && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character targetCharacter && contextMenuUIController.currentlyOpenedParentContextItem is TriggerFlawData) {
            OnHoverEnterFlaw(trait.name,  targetCharacter, p_hoverPosition);
        }
    }
    private void OnHoverOutPlayerActionContextMenuItem(IContextMenuItem p_item) {
        if (p_item is PlayerAction playerAction) {
            PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
        } else {
            HideSmallInfo();    
        }
        
    }
    private void OnHoverEnterFlaw(string traitName, Character p_character, UIHoverPosition p_hoverPosition) {
        Trait trait = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
        string title = traitName;
        string fullDescription = trait.GetTriggerFlawEffectDescription(p_character, "flaw_effect");
        if (string.IsNullOrEmpty(fullDescription)) {
            fullDescription = "This flaw does not have a trigger flaw effect.";
        }
        if (p_character.isInfoUnlocked) {
            int manaCost = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost;
            string currencyStr = string.Empty;
            if (manaCost != -1) {
                currencyStr = $"{currencyStr}{manaCost.ToString()}{UtilityScripts.Utilities.ManaIcon()}  ";
            }
            title = $"{title}    <size=16>{currencyStr}";
            string additionalText = string.Empty;
            if (PlayerManager.Instance.player.mana < manaCost) {
                additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Not enough mana.")}\n";
            }
            fullDescription = $"{fullDescription}\n\n{additionalText}";
        } else {
            title = "Unknown flaw";
            fullDescription = $"{"Trigger "}{ UtilityScripts.Utilities.ColorizeName(p_character.name)}{"\'s unknown flaw"}";
        }
      
        ShowSmallInfo(fullDescription, pos: p_hoverPosition, header: title, autoReplaceText: false);
    }
    private void OnHoverPlayerAction(SkillData spellData, UIHoverPosition p_hoverPosition, IPlayerActionTarget p_target) {
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData, p_hoverPosition);
        return;
        
        //PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
        //string title = $"{spellData.name}";
        //string fullDescription = spellData.description;
        //int charges = spellData.charges;
        //int manaCost = playerSkillData.GetManaCostBaseOnLevel(spellData.currentLevel);
        //int cooldown = playerSkillData.GetCoolDownBaseOnLevel(spellData.currentLevel);

        //string currencyStr = string.Empty; 
        
        //if (manaCost != -1) {
        //    currencyStr = $"{currencyStr}{manaCost.ToString()}{UtilityScripts.Utilities.ManaIcon()}  ";
        //}
        //if (charges != -1) {
        //    currencyStr = $"{currencyStr}{charges.ToString()}/{playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel).ToString()}{UtilityScripts.Utilities.ChargesIcon()}  ";
        //}
        //if (cooldown != -1) {
        //    currencyStr = $"{currencyStr}{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)}{UtilityScripts.Utilities.CooldownIcon()}  ";
        //}
        //if (spellData.threat > 0) {
        //    currencyStr = $"{currencyStr}{spellData.threat.ToString()}{UtilityScripts.Utilities.ThreatIcon()}  ";
        //}
        //title = $"{title}    <size=16>{currencyStr}";

        //string additionalText = string.Empty;
        //if (spellData is PlayerAction) {
        //    IPlayerActionTarget activePOI = p_target;
        //    if (activePOI != null) {
        //        if (activePOI is Character activeCharacter) {
        //            if (spellData.CanPerformAbilityTowards(activeCharacter) == false) {
        //                if (spellData is PlayerAction playerAction && !playerAction.canBeCastOnBlessed && activeCharacter.traitContainer.IsBlessed()) {
        //                    additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Blessed Villagers are protected from your powers.")}\n";
        //                }
        //                string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeCharacter);
        //                wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
        //                additionalText += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
        //            }
        //        } else if (activePOI is TileObject activeTileObject) {
        //            if (activeTileObject is AnkhOfAnubis ankh && ankh.isActivated && spellData.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT) {
        //                additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Activated Ankh can no longer be seized.")}\n";
        //            }
        //            string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeTileObject);
        //            wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
        //            additionalText += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
        //        } else if (activePOI is BaseSettlement activeSettlement) {
        //            if (spellData.CanPerformAbilityTowards(activeSettlement) == false) {
        //                string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeSettlement);
        //                wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
        //                additionalText += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
        //            }
        //        } else if (activePOI is LocationStructure activeStructure) {
        //            if (spellData.CanPerformAbilityTowards(activeStructure) == false) {
        //                string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeStructure);
        //                wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
        //                additionalText += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
        //            }
        //        }
        //    }
        //}
        //if(HasEnoughMana(spellData) == false) {
        //    additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Not enough mana.")}\n";
        //}
        //if(HasEnoughCharges(spellData) == false) {
        //    if (spellData.hasCooldown) {
        //        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Recharging.")}\n";
        //    } else {
        //        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Not enough charges.")}\n";
        //    }
        //}
        //if (spellData is BrainwashData && p_target is Character targetCharacter) {
        //    fullDescription = $"{fullDescription}\n<b>{targetCharacter.name} Brainwash Success Rate: {PrisonCell.GetBrainwashSuccessRate(targetCharacter).ToString("N0")}%</b>";
        //}

        //fullDescription = $"{fullDescription}\n\n{additionalText}";
        //ShowSmallInfo(fullDescription, pos: p_hoverPosition, header: title, autoReplaceText: false);
    }
    private bool HasEnoughMana(SkillData spellData) {
        if (spellData.hasManaCost) {
            if (PlayerManager.Instance.player.mana >= spellData.manaCost) {
                return true;
            }
            return false;
        }
        //if skill has no mana cost then always has enough mana
        return true;
    }
    private bool HasEnoughCharges(SkillData spellData) {
        if (spellData.hasCharges) {
            if (spellData.charges > 0) {
                return true;
            }
            return false;
        }
        //if skill has no charges then always has enough charges
        return true;
    }
    private void PopulatePlayerActionContextMenuItems(List<IContextMenuItem> contextMenuItems, IPlayerActionTarget p_target) {
        for (int i = 0; i < p_target.actions.Count; i++) {
            PLAYER_SKILL_TYPE skillType = p_target.actions[i];
            PlayerAction playerAction = PlayerSkillManager.Instance.GetSkillData(skillType) as PlayerAction;
            if(playerAction != null && playerAction.shouldShowOnContextMenu) {
                if (playerAction.IsValid(p_target) && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(skillType)) {
                    contextMenuItems.Add(playerAction);
                }
            }
        }
    }
    private void AddPlayerActionContextMenuSignals() {
        Messenger.AddListener<IPlayerActionTarget>(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
        Messenger.AddListener(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS, ForceReloadPlayerActions);
        Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
        Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
    }
    private void RemovePlayerActionContextMenuSignals() {
        Messenger.RemoveListener<IPlayerActionTarget>(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
        Messenger.RemoveListener(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS, ForceReloadPlayerActions);
        Messenger.RemoveListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
        Messenger.RemoveListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
    }
    private void OnPlayerActionRemovedFromTarget(PLAYER_SKILL_TYPE p_skillType, IPlayerActionTarget p_target) {
        if (IsContextMenuShowing() && p_target != null && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target) {
            UpdatePlayerActionContextMenuItems(p_target);
        }
    }
    private void OnPlayerActionAddedToTarget(PLAYER_SKILL_TYPE p_skillType, IPlayerActionTarget p_target) {
        if (IsContextMenuShowing() && p_target != null && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target) {
            UpdatePlayerActionContextMenuItems(p_target);
        }
    }
    private void ReloadPlayerActions(IPlayerActionTarget p_target) {
        if (IsContextMenuShowing() && p_target != null && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target) {
            UpdatePlayerActionContextMenuItems(p_target);
        }
    }
    private void ForceReloadPlayerActions() {
        if (IsContextMenuShowing() && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null) {
            UpdatePlayerActionContextMenuItems(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
        }
    }
    private void UpdatePlayerActionContextMenuItems(IPlayerActionTarget p_target) {
        List<IContextMenuItem> contextMenuItems = RuinarchListPool<IContextMenuItem>.Claim();
        PopulatePlayerActionContextMenuItems(contextMenuItems, p_target);
        //List<IContextMenuItem> contextMenuItems = GetPlayerActionContextMenuItems(p_target);
        // if (contextMenuItems == null) {
        //     HidePlayerActionContextMenu();
        //     return;
        // }
        contextMenuUIController.UpdateContextMenuItems(contextMenuItems);
        RuinarchListPool<IContextMenuItem>.Release(contextMenuItems);
    }
    private bool IsMouseOnContextMenu() {
        if (_pointer != null) {
            _pointer.position = Input.mousePosition;
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(_pointer, _raycastResults);

            return _raycastResults.Count > 0 && _raycastResults.Any(go => go.gameObject.CompareTag("Context Menu"));    
        }
        return false;
    }
    #endregion

    #region Demonic Structures
    [Space(10)]
    [Header("Demonic Structures")]
    [SerializeField] private PortalUIController _portalUIController;
    public UpgradePortalUIController upgradePortalUIController;
    public PurchaseSkillUIController purchaseSkillUIController;
    public void ShowUnlockAbilitiesUI(ThePortal portal) {
        _portalUIController.ShowUI(portal);
        SetSpeedTogglesState(false);
        Pause();
    }
    public void ShowUpgradeAbilitiesUI() {
        onSpireClicked?.Invoke();
    }
    public void ShowRaidUI(LocationStructure structure) {
        onMaraudClicked?.Invoke(structure);
    }
    public void ShowSnatchVillagerUI(LocationStructure structure) {
        onTortureChamberClicked?.Invoke(structure);
    }
    public void ShowSnatchMonsterUI(LocationStructure structure) {
        onKennelClicked?.Invoke(structure);
    }
    public void ShowDefendUI(LocationStructure structure) {
        onDefensePointClicked?.Invoke(structure);
    }
    public void ShowUpgradePortalUI(ThePortal portal) {
        upgradePortalUIController.ShowPortalUpgradeTier(portal.nextTier, portal.level, portal);
    }
    public void ShowPurchaseSkillUI() {
        purchaseSkillUIController.Init(3, true);
    }
    #endregion

    #region Bookmarks UI
    [Space(10)]
    [Header("Bookmark UI")]
    public BookmarkUIController bookmarkUIController;
    public void OnBookmarkMenuHide() {
        (notificationHoverPos.transform as RectTransform).anchoredPosition = notificationHoverPosDefaultPosition;
    }
    public void OnBookmarkMenuShow() {
        (notificationHoverPos.transform as RectTransform).anchoredPosition = notificationHoverPosModifiedPosition;
    }
    #endregion

    #region Wait Window
    [Header("Wait Window")] 
    [SerializeField] private GameObject waitWindow;
    public void ShowWaitForTileObjectGenerationToFinishWindow() {
        Pause();
        SetSpeedTogglesState(false);
        InnerMapCameraMove.Instance.DisableMovement();
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        waitWindow.SetActive(true);
        StartCoroutine(WaitForTileObjectGenerationToFinish());
    }
    private void HideWaitForTileObjectGenerationToFinishWindow() {
        SetSpeedTogglesState(true);
        InnerMapCameraMove.Instance.EnableMovement();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
        waitWindow.SetActive(false);
    }
    private IEnumerator WaitForTileObjectGenerationToFinish() {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while (WorldConfigManager.Instance.mapGenerationData.isGeneratingTileObjects) {
            yield return null;
        }
        HideWaitForTileObjectGenerationToFinishWindow();
        GameManager.Instance.StartProgression();
        stopwatch.Stop();
        Debug.Log($"WaitForTileObjectGenerationToFinish took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
    }
    public bool IsWaitingForTileObjectGenerationToComplete() {
        return waitWindow.activeInHierarchy;
    }
    #endregion

    #region Load Window
    public void OpenLoadWindow() {
        loadWindow.Open();
    }
    private void OnLoadHotkeyPressed() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving || SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk) {
            //prevent opening load window while player is currently saving
            return;
        }
        if (!IsLoadWindowShowing()) {
            OpenLoadWindow();    
        }
    }
    public bool IsLoadWindowShowing() {
        return loadWindow.isShowing;
    }
    public void CloseLoadWindow() {
        loadWindow.Close();
    }
    #endregion

}