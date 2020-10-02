using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Factions;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Logs;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UtilityScripts;

public class UIManager : BaseMonoBehaviour {

    public static UIManager Instance = null;

    public const string normalTextColor = "#CEB67C";
    public const string buffTextColor = "#39FF14";
    public const string flawTextColor = "#FF073A";

    public RectTransform mainRT;
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

    [Header("Small Info with Visual")] 
    [SerializeField] private SmallInfoWithVisual _smallInfoWithVisual;
    
    [Header("Character Nameplate Tooltip")]
    [SerializeField] private CharacterNameplateItem _characterNameplateTooltip;
    
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
    [Header("World UI")]
    [SerializeField] private RectTransform worldUIParent;
    [SerializeField] private GraphicRaycaster worldUIRaycaster;

    [Space(10)]
    [Header("Object Picker")]
    [SerializeField] private ObjectPicker objectPicker;
    
    [Space(10)]
    [Header("Right Click Commands")]
    public POITestingUI poiTestingUI;
    public MinionCommandsUI minionCommandsUI;

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

    [Header("Transition Region UI")]
    public GameObject transitionRegionUIGO;
    public RuinarchButton rightTransitionBtn;
    public RuinarchButton leftTransitionBtn;
    public RuinarchButton upTransitionBtn;
    public RuinarchButton downTransitionBtn;

    public bool isShowingAreaTooltip { get; private set; } //is the tooltip for npcSettlement double clicks showing?
    public PopupMenuBase latestOpenedPopup { get; private set; }
    public InfoUIBase latestOpenedInfoUI { get; private set; }
    private InfoUIBase _lastOpenedInfoUI;
    //public List<PopupMenuBase> openedPopups { get; private set; }
    private PointerEventData _pointer;
    private List<RaycastResult> _raycastResults;
    
    public bool tempDisableShowInfoUI { get; private set; }

    public List<UnallowOverlaps> unallowOverlaps;
    
    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        //openedPopups = new List<PopupMenuBase>();
        Messenger.AddListener<bool>(Signals.PAUSED, UpdateSpeedToggles);
        Messenger.AddListener(Signals.UPDATE_UI, UpdateUI);
        Messenger.AddListener(Signals.INSPECT_ALL, UpdateInteractableInfoUI);
    }
    private void Update() {
        if (isHoveringTile) {
            currentTileHovered.region?.OnHoverOverAction();
        }
        UpdateTransitionRegionUI();
    }
    #endregion

    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    internal void InitializeUI() {
        _pointer = new PointerEventData(EventSystem.current);
        unallowOverlaps = new List<UnallowOverlaps>();
        _raycastResults = new List<RaycastResult>();
        allMenus = transform.GetComponentsInChildren<InfoUIBase>(true);
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
        }
        openedPopups = new List<PopupMenuBase>();
        questUI.Initialize();
        Messenger.AddListener(Signals.HIDE_MENUS, HideMenus);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, ShowDeveloperNotification);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);
        
        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
 
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);

        Messenger.AddListener<IIntel>(Signals.SHOW_INTEL_NOTIFICATION, ShowPlayerNotification);
        Messenger.AddListener<Log>(Signals.SHOW_PLAYER_NOTIFICATION, ShowPlayerNotification);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnUIMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnUIMenuClosed);
        
        Messenger.AddListener<PopupMenuBase>(Signals.POPUP_MENU_OPENED, OnPopupMenuOpened);
        Messenger.AddListener<PopupMenuBase>(Signals.POPUP_MENU_CLOSED, OnPopupMenuClosed);
        
        Messenger.AddListener<IPointOfInterest>(Signals.UPDATE_POI_LOGS_UI, TryUpdatePOILog);
        Messenger.AddListener<Faction>(Signals.UPDATE_FACTION_LOGS_UI, TryUpdateFactionLog);

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
        
        UpdateUI();
        // && WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Oona
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
        returnToWorldBtn.gameObject.SetActive(WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial);
    }
    private void HideMenus() {
        // poiTestingUI.HideUI();
        // minionCommandsUI.HideUI();
        // customDropdownList.Close();
        // if (characterInfoUI.isShowing) {
        //     characterInfoUI.CloseMenu();
        // }
        // if (factionInfoUI.isShowing) {
        //     factionInfoUI.CloseMenu();
        // }
        // if (regionInfoUI.isShowing) {
        //     regionInfoUI.CloseMenu();
        // }
        // if (tileObjectInfoUI.isShowing) {
        //     tileObjectInfoUI.CloseMenu();
        // }
        // if (objectPicker.gameObject.activeSelf) {
        //     HideObjectPicker();
        // }
        // if (PlayerUI.Instance.isShowingKillSummary) {
        //     PlayerUI.Instance.HideKillSummary();
        // }
        // if (PlayerUI.Instance.isShowingMinionList) {
        //     PlayerUI.Instance.HideMinionList();
        // }
        // if (hexTileInfoUI.isShowing) {
        //     hexTileInfoUI.CloseMenu();
        // }
        // if (structureInfoUI.isShowing) {
        //     structureInfoUI.CloseMenu();
        // }
    }
    private void UpdateUI() {
        dateLbl.SetText(
            $"Day {GameManager.Instance.continuousDays.ToString()}\n{GameManager.ConvertTickToTime(GameManager.Instance.Today().tick)}");

        UpdateInteractableInfoUI();
        //UpdateFactionInfo();
        PlayerUI.Instance.UpdateUI();
    }
    private void UpdateInteractableInfoUI() {
        UpdateCharacterInfo();
        UpdateMonsterInfo();
        UpdateTileObjectInfo();
        UpdateRegionInfo();
        UpdateHextileInfo();
        UpdateStructureInfo();
        UpdatePartyInfo();
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
        if (!x1Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
    }
    public void SetProgressionSpeed2X() {
        if (!x2Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
    }
    public void SetProgressionSpeed4X() {
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
        Pause();
        // Debug.Log("Game was paused by player.");
        Messenger.Broadcast(Signals.PAUSED_BY_PLAYER);
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

    #region Minimap
    internal void UpdateMinimapInfo() {
        //CameraMove.Instance.UpdateMinimapTexture();
    }
    #endregion

    #region Tooltips
    public void ShowSmallInfo(string info, string header = "", bool autoReplaceText = true) {
        Profiler.BeginSample("Show Small Info Sample");
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
            smallInfoGO.transform.SetParent(this.transform);
            smallInfoGO.SetActive(true);
            if (gameObject.activeInHierarchy) {
                StartCoroutine(ReLayout(smallInfoBGParentLG));
                StartCoroutine(ReLayout(smallInfoVerticalLG));    
            }
        }
        PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
        Profiler.EndSample();
    }
    public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "", bool autoReplaceText = true) {
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
            smallInfoGO.SetActive(true);
            if (gameObject.activeInHierarchy) {
                StartCoroutine(ReLayout(smallInfoBGParentLG));
                StartCoroutine(ReLayout(smallInfoVerticalLG));    
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
        if (Settings.SettingsManager.Instance.settings.doNotShowVideos) {
            ShowSmallInfo(info, pos, header);
        } else {
            _smallInfoWithVisual.ShowSmallInfo(info, videoClip, header, pos);
        }
    }
    public void ShowSmallInfo(string info, Texture visual, string header = "", UIHoverPosition pos = null) {
        Assert.IsNotNull(visual, "Small info with visual was called but no visual was provided");
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

        characterPortraitHoverInfoRT.SetParent(this.transform);
        PositionTooltip(characterPortraitHoverInfoRT.gameObject, characterPortraitHoverInfoRT, characterPortraitHoverInfoRT);
    }
    public void HideCharacterPortraitHoverInfo() {
        characterPortraitHoverInfoGO.SetActive(false);
    }
    public void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        PositionTooltip(Input.mousePosition, tooltipParent, rtToReposition, boundsRT);
    }
    public void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        var v3 = position;

        rtToReposition.pivot = new Vector2(0f, 1f);
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
        
        tooltipParent.transform.position = v3;

        if (rtToReposition.sizeDelta.y >= Screen.height) {
            return;
        }

        Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        List<int> cornersOutside = new List<int>();
        boundsRT.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++) {
            Vector3 localSpacePoint = mainRT.InverseTransformPoint(corners[i]);
            // If parent (canvas) does not contain checked items any point
            if (!mainRT.rect.Contains(localSpacePoint)) {
                cornersOutside.Add(i);
            }
        }

        if (cornersOutside.Count != 0) {
            if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
                if (cornersOutside.Contains(0)) {
                    //bottom side and right side are outside, move anchor to bottom right
                    rtToReposition.pivot = new Vector2(1f, 0f);
                    smallInfoBGParentLG.childAlignment = TextAnchor.LowerRight;
                } else {
                    //right side is outside, move anchor to top right side
                    rtToReposition.pivot = new Vector2(1f, 1f);
                    smallInfoBGParentLG.childAlignment = TextAnchor.UpperRight;
                }
            } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                //bottom side is outside, move anchor to bottom left
                rtToReposition.pivot = new Vector2(0f, 0f);
                smallInfoBGParentLG.childAlignment = TextAnchor.LowerLeft;
            }
            rtToReposition.localPosition = Vector3.zero;
        }
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
    public void ShowSmallLocationInfo(Region region, RectTransform initialParent, Vector2 adjustment, string subText = "") {
        locationSmallInfo.ShowRegionInfo(region, subText);
        locationSmallInfoRT.SetParent(initialParent);
        locationSmallInfoRT.anchoredPosition = Vector3.zero;
        locationSmallInfoRT.anchoredPosition += adjustment;
        locationSmallInfoRT.SetParent(this.transform);
        //(locationSmallInfo.transform as RectTransform).anchoredPosition = pos;
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
    #endregion

    #region Developer Notifications NPCSettlement
    private void ShowDeveloperNotification(string text, int expirationTicks, UnityAction onClickAction) {
        developerNotificationArea.ShowNotification(text, expirationTicks, onClickAction);
    }
    #endregion

    #region World History
    internal void AddLogToLogHistory(Log log) {
        Messenger.Broadcast<Log>("AddLogToHistory", log);
    }
    public void ToggleNotificationHistory() {
        //worldHistoryUI.ToggleWorldHistoryUI();
        //if (notificationHistoryGO.activeSelf) {
        //    HideNotificationHistory();
        //} else {
        //    ShowLogHistory();
        //}
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
        _pointer.position = Input.mousePosition;
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointer, _raycastResults);

        return _raycastResults.Count > 0 && _raycastResults.Any(
            go => go.gameObject.layer == LayerMask.NameToLayer("UI") || 
                  go.gameObject.layer == LayerMask.NameToLayer("WorldUI") || 
                  go.gameObject.CompareTag("Map_Click_Blocker"));
    }
    public void OpenObjectUI(object obj) {
        if (obj is Character character) {
            ShowCharacterInfo(character, true);
        } else if (obj is NPCSettlement settlement) {
            ShowRegionInfo(settlement.region);
        } else if (obj is Faction faction) {
            ShowFactionInfo(faction);
        } else if (obj is Minion minion) {
            ShowCharacterInfo(minion.character, true);
        } else if (obj is Party party) {
            ShowPartyInfo(party);
        } else if (obj is TileObject tileObject) {
            ShowTileObjectInfo(tileObject);
        } else if (obj is Region region) {
            ShowRegionInfo(region);
        } else if (obj is LocationStructure structure) {
            structure.CenterOnStructure();
        }
    }
    public bool IsMouseOnMapObject() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
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
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                if (go.gameObject.layer == LayerMask.NameToLayer("UI") || go.gameObject.layer == LayerMask.NameToLayer("WorldUI") 
                    || go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    return true;
                }
            }
        }
        return false;
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

    #region Nameplate
    public LandmarkNameplate CreateLandmarkNameplate(BaseLandmark landmark) {
        GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("LandmarkNameplate", worldUIParent);
        nameplateGO.transform.localScale = Vector3.one;
        LandmarkNameplate nameplate = nameplateGO.GetComponent<LandmarkNameplate>();
        nameplate.SetLandmark(landmark);
        return nameplate;
    }
    #endregion

    #region Object Picker
    public void ShowClickableObjectPicker<T>(List<T> choices, Action<object> onClickAction, IComparer<T> comparer = null
        , Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverAction = null, Action<T> onHoverExitAction = null, 
        string identifier = "", bool showCover = false, int layer = 9, bool closable = true, Func<string,Sprite> portraitGetter = null, bool shouldShowConfirmationWindowOnPick = false, bool asButton = false) {

        objectPicker.ShowClickable(choices, onClickAction, comparer, validityChecker, title, onHoverAction,
            onHoverExitAction, identifier, showCover, layer, portraitGetter, asButton, shouldShowConfirmationWindowOnPick);
        Messenger.Broadcast(Signals.OBJECT_PICKER_SHOWN, identifier);
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
        this.gameObject.SetActive(state);
        Messenger.Broadcast(Signals.UI_STATE_SET);
    }
    public void DateHover() {
        ShowSmallInfo($"Day: {GameManager.Instance.continuousDays.ToString()} Tick: {GameManager.Instance.Today().tick.ToString()}");
    }
    [ExecuteInEditMode]
    [ContextMenu("Set All Scroll Rect Scroll Speed")]
    public void SetAllScrollSpeed() {
        ScrollRect[] allScroll = this.gameObject.GetComponentsInChildren<ScrollRect>(true);
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
        if (GameManager.Instance.gameHasStarted == false) {
            return;
        }
        Character characterToShow = character;
        if(character.lycanData != null) {
            characterToShow = character.lycanData.activeForm;
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

    #region Region Info
    [Space(10)]
    [Header("Region Info")] public RegionInfoUI regionInfoUI;
    public void ShowRegionInfo(Region region, bool centerOnRegion = true) {
        regionInfoUI.SetData(region);
        regionInfoUI.OpenMenu();

        if (centerOnRegion) {
            region.CenterCameraOnRegion();
            region.ShowBorders(Color.yellow, true);
        }
    }
    public void UpdateRegionInfo() {
        if (regionInfoUI.isShowing) {
            regionInfoUI.UpdateInfo();
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

    #region Tile Info
    [Space(10)]
    [Header("Tile Info")]
    [SerializeField] public HextileInfoUI hexTileInfoUI;
    public void ShowHexTileInfo(HexTile item) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        hexTileInfoUI.SetData(item);
        hexTileInfoUI.OpenMenu();
    }
    public void UpdateHextileInfo() {
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.UpdateHexTileInfo();
        }
    }
    #endregion
    
    #region Structure Info
    [Space(10)]
    [Header("Structure Info")]
    [SerializeField] public StructureInfoUI structureInfoUI;
    public void ShowStructureInfo(LocationStructure structure) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        structureInfoUI.SetData(structure);
        structureInfoUI.OpenMenu();
    }
    public void UpdateStructureInfo() {
        if (structureInfoUI.isShowing) {
            structureInfoUI.UpdateStructureInfoUI();
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

    #region Tile Hover
    //private HexTile previousTileHovered;
    private HexTile currentTileHovered;
    private float timeHovered;
    private const float hoverThreshold = 1.5f;
    private bool isHoveringTile = false;
    private void OnHoverOverTile(HexTile tile) {
        //previousTileHovered = currentTileHovered;
        currentTileHovered = tile;
        isHoveringTile = true;
    }
    public void OnHoverOutTile(HexTile tile) {
        currentTileHovered = null;
        isHoveringTile = false;
        tile.region?.OnHoverOutAction();
        if (tile.region != null) {
            HideSmallInfo();
            isShowingAreaTooltip = false;
        }
    }
    #endregion

    #region Inner Map
    [Header("Inner Maps")]
    [SerializeField] private Button returnToWorldBtn;
    [SerializeField] private UIHoverPosition returnToWorldBtnTooltipPos;
    private void OnInnerMapOpened(Region location) {
        worldUIRaycaster.enabled = false;
        bottomNotification.HideMessage();
    }
    private void OnInnerMapClosed(Region location) {
        worldUIRaycaster.enabled = true;
        bottomNotification.ShowMessage("Click on any tile to go there.");
    }

    public void ToggleBetweenMaps() {
        if (InnerMapManager.Instance.isAnInnerMapShowing) {
            InnerMapManager.Instance.HideAreaMap();
            OnCameraOutOfFocus();
        } else {
            if(regionInfoUI.activeRegion != null) {
                InnerMapManager.Instance.TryShowLocationMap(regionInfoUI.activeRegion);
            } else if(hexTileInfoUI.currentlyShowingHexTile != null) {
                InnerMapManager.Instance.TryShowLocationMap(hexTileInfoUI.currentlyShowingHexTile.region);
                InnerMapCameraMove.Instance.CenterCameraOnTile(hexTileInfoUI.currentlyShowingHexTile);
            }
        }
    }
    public void ToggleMapsHover() {
        if (InnerMapManager.Instance.isAnInnerMapShowing) {
            ShowSmallInfo($"Click to exit {InnerMapManager.Instance.currentlyShowingLocation.name}.", returnToWorldBtnTooltipPos);
        } else {
            if (regionInfoUI.activeRegion != null) {
                ShowSmallInfo($"Click to enter {regionInfoUI.activeRegion.name}.", returnToWorldBtnTooltipPos);
            } else if(hexTileInfoUI.currentlyShowingHexTile != null) {
                ShowSmallInfo($"Click to enter {hexTileInfoUI.currentlyShowingHexTile.region.name}.", returnToWorldBtnTooltipPos);
            }
        }
    }
    #endregion

    #region Share Intel
    [Header("Share Intel")]
    [SerializeField] private ShareIntelMenu shareIntelMenu;
    public void OpenShareIntelMenu(Character targetCharacter, Character actor, IIntel intel) {
        shareIntelMenu.Open(targetCharacter, actor, intel);
    }
    public bool IsShareIntelMenuOpen() {
        return shareIntelMenu.gameObject.activeSelf;
    }
    public void CloseShareIntelMenu() {
        shareIntelMenu.Close();
    }
    private void OnOpenShareIntelMenu() {
        returnToWorldBtn.interactable = false;
        SetCoverState(true);
        //playerNotificationParent.SetSiblingIndex(1);
    }
    private void OnCloseShareIntelMenu() {
        returnToWorldBtn.interactable = true;
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
    private void PlaceNewNotification(PlayerNotificationItem newNotif, in Log shownLog) {
        //check if the log used is from a GoapAction
        //then check all other currently showing notifications, if it is from the same goap action
        //replace that log with this new one
        PlayerNotificationItem itemToReplace = null;
        if (shownLog.hasValue && !string.IsNullOrEmpty(shownLog.actionID)) {
            for (int i = 0; i < activeNotifications.Count; i++) {
                PlayerNotificationItem currItem = activeNotifications[i];
                if (!string.IsNullOrEmpty(currItem.fromActionID) && shownLog.actionID == currItem.fromActionID) {
                    itemToReplace = currItem;
                    break;
                }
            }
        }
        if (itemToReplace != null) {
            itemToReplace.DeleteNotification();
        }
        activeNotifications.Add(newNotif);
        activeNotificationIDs.Add(shownLog.persistentID);
        if (activeNotifications.Count > maxPlayerNotif) {
            activeNotifications[0].DeleteOldestNotification();
        }
        UpdateSearchFieldsState();
        // if (HasSearchCriteria()) {
            List<string> filteredLogIDs = DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, notificationSearchField.text, notificationFilters, 30);
            if (filteredLogIDs.Contains(newNotif.logPersistentID)) {
                newNotif.DoTweenHeight();
                newNotif.TweenIn();    
            } else {
                newNotif.QueueAdjustHeightOnEnable();
            }
            FilterNotifications(filteredLogIDs);
        // } else {
        //     newNotif.DoTweenHeight();
        //     newNotif.TweenIn();    
        // }
    }
    private void OnNotificationDestroyed(PlayerNotificationItem item) {
        activeNotifications.Remove(item);
        activeNotificationIDs.Remove(item.logPersistentID);
        UpdateSearchFieldsState();
    }
    private void FilterNotifications(List<string> filteredLogIDs = null) {
        if (filteredLogIDs == null) {
            filteredLogIDs = DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, notificationSearchField.text, notificationFilters, 30);
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
    public void ShowYesNoConfirmation(string header, string question, System.Action onClickYesAction = null, System.Action onClickNoAction = null,
        bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, bool pauseAndResume = false, 
        bool yesBtnActive = true, bool noBtnActive = true, System.Action yesBtnInactiveHoverAction = null, System.Action yesBtnInactiveHoverExitAction = null) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowYesNoConfirmation(header, question, onClickYesAction, onClickNoAction, 
                showCover, layer, yesBtnText, noBtnText, yesBtnInteractable, noBtnInteractable, pauseAndResume,
                yesBtnActive, noBtnActive, yesBtnInactiveHoverAction, yesBtnInactiveHoverExitAction));
            return;
        }
        
        if (pauseAndResume && !IsObjectPickerOpen()) {
            //if object picker is already being shown, do not pause, so that this does not mess with the previously set speed. 
            Pause();
            SetSpeedTogglesState(false);    
        }
        yesNoConfirmation.ShowYesNoConfirmation(header, question, onClickYesAction, onClickNoAction, showCover, layer, yesBtnText, noBtnText, yesBtnInteractable, noBtnInteractable,  pauseAndResume, 
            yesBtnActive, noBtnActive, yesBtnInactiveHoverAction, yesBtnInactiveHoverExitAction);
    }
    public void HideYesNoConfirmation() {
        yesNoConfirmation.HideYesNoConfirmation();
        if (!PlayerUI.Instance.TryShowPendingUI() && !IsObjectPickerOpen() && !optionsMenu.isShowing) {
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
    public void ShowTriggerFlawConfirmation(string question, string effect, string manaCost, System.Action onClickYesAction = null,
    bool showCover = false, int layer = 21, bool pauseAndResume = false) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowTriggerFlawConfirmation(question, effect, manaCost, onClickYesAction, showCover, layer, pauseAndResume));
            return;
        }
        if (pauseAndResume && !IsObjectPickerOpen()) {
            //if object picker is already being shown, do not pause, so that this does not mess with the previously set speed. 
            Pause();
            SetSpeedTogglesState(false);    
        }
        
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
    public void ShowImportantNotification(GameDate date, string message, System.Action onClickAction) {
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
        minionCardTooltip.transform.SetParent(this.transform);
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

    #region Demo
    [Header("Demo")]
    [SerializeField] private DemoUI _demoUI;
    public void ShowStartDemoScreen() {
        _demoUI.ShowStartScreen();
    }
    public void ShowEndDemoScreen(string summary) {
        _demoUI.ShowSummaryThenEndScreen(summary);
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
        throw new System.Exception($"No Log tag sprite for tag {tag.ToString()}");
    }
    #endregion

    #region Transition Region UI
    private void UpdateTransitionRegionUI() {
        if (InnerMapManager.Instance.currentlyShowingLocation != null) {
            transitionRegionUIGO.SetActive(true);
            if (InnerMapCameraMove.Instance.HasReachedMinXBound()) {
                if (InnerMapManager.Instance.currentlyShowingLocation.HasNeighbourInDirection(GridNeighbourDirection.West)) {
                    leftTransitionBtn.gameObject.SetActive(true);
                } else {
                    leftTransitionBtn.gameObject.SetActive(false);
                }
            } else {
                leftTransitionBtn.gameObject.SetActive(false);
            }
            if (InnerMapCameraMove.Instance.HasReachedMaxXBound()) {
                if (InnerMapManager.Instance.currentlyShowingLocation.HasNeighbourInDirection(GridNeighbourDirection.East)) {
                    rightTransitionBtn.gameObject.SetActive(true);
                } else {
                    rightTransitionBtn.gameObject.SetActive(false);
                }
            } else {
                rightTransitionBtn.gameObject.SetActive(false);
            }
            if (InnerMapCameraMove.Instance.HasReachedMinYBound()) {
                if (InnerMapManager.Instance.currentlyShowingLocation.HasNeighbourInDirection(GridNeighbourDirection.South)) {
                    downTransitionBtn.gameObject.SetActive(true);
                } else {
                    downTransitionBtn.gameObject.SetActive(false);
                }
            } else {
                downTransitionBtn.gameObject.SetActive(false);
            }
            if (InnerMapCameraMove.Instance.HasReachedMaxYBound()) {
                if (InnerMapManager.Instance.currentlyShowingLocation.HasNeighbourInDirection(GridNeighbourDirection.North)) {
                    upTransitionBtn.gameObject.SetActive(true);
                } else {
                    upTransitionBtn.gameObject.SetActive(false);
                }
            } else {
                upTransitionBtn.gameObject.SetActive(false);
            }
        } else {
            transitionRegionUIGO.SetActive(false);
        }
    }
    public void OnClickRegionTransition(string direction) {
        if (InnerMapManager.Instance.currentlyShowingLocation != null) {
            GridNeighbourDirection dir = (GridNeighbourDirection) System.Enum.Parse(typeof(GridNeighbourDirection), direction);
            Region region = InnerMapManager.Instance.currentlyShowingLocation.GetNeighbourInDirection(dir);
            if(region != null) {
                InnerMapManager.Instance.TryShowLocationMap(region);
                InnerMapCameraMove.Instance.CenterCameraOnTile(region.coreTile);
            }
        }
    }
    public void OnHoverRegionTransitionBtn(string direction) {
        if (InnerMapManager.Instance.currentlyShowingLocation != null) {
            GridNeighbourDirection dir = (GridNeighbourDirection) System.Enum.Parse(typeof(GridNeighbourDirection), direction);
            Region region = InnerMapManager.Instance.currentlyShowingLocation.GetNeighbourInDirection(dir);
            if (region != null) {
                ShowSmallInfo("Go To " + region.name);
            }
        }
    }
    public void OnHoverOutRegionTransitionBtn(string direction) {
        HideSmallInfo();
    }
    #endregion

    #region Overlap UI
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
}