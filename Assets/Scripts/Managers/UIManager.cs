using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;

public class UIManager : MonoBehaviour {

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
    public RectTransform smallInfoBGRT;
    public TextMeshProUGUI smallInfoLbl;
    public EnvelopContentUnityUI smallInfoEnvelopContent;
    public LocationSmallInfo locationSmallInfo;
    public RectTransform locationSmallInfoRT;
    public GameObject characterPortraitHoverInfoGO;
    public CharacterPortrait characterPortraitHoverInfo;
    public RectTransform characterPortraitHoverInfoRT;

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
    public GameObject unsummonedMinionNameplatePrefab;
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

    public bool isShowingAreaTooltip { get; private set; } //is the tooltip for npcSettlement double clicks showing?
    private InfoUIBase _lastOpenedInfoUI;
    public List<PopupMenuBase> openedPopups { get; private set; }
    private PointerEventData _pointer;
    private List<RaycastResult> _raycastResults;
    
    public bool tempDisableShowInfoUI { get; private set; }

    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        openedPopups = new List<PopupMenuBase>();
        Messenger.AddListener<bool>(Signals.PAUSED, UpdateSpeedToggles);
        Messenger.AddListener(Signals.UPDATE_UI, UpdateUI);
        Messenger.AddListener(Signals.INSPECT_ALL, UpdateInteractableInfoUI);
    }
    private void Update() {
        if (isHoveringTile) {
            currentTileHovered.region?.OnHoverOverAction();
        }
    }
    #endregion
    
    internal void InitializeUI() {
        _pointer = new PointerEventData(EventSystem.current);
        _raycastResults = new List<RaycastResult>();
        allMenus = transform.GetComponentsInChildren<InfoUIBase>(true);
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
        }
        questInfoUI.Initialize();
        Messenger.AddListener(Signals.HIDE_MENUS, HideMenus);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, ShowDeveloperNotification);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);
        
        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);

        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);

        Messenger.AddListener<Intel>(Signals.SHOW_INTEL_NOTIFICATION, ShowPlayerNotification);
        Messenger.AddListener<Log>(Signals.SHOW_PLAYER_NOTIFICATION, ShowPlayerNotification);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnUIMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnUIMenuClosed);
        
        Messenger.AddListener<PopupMenuBase>(Signals.POPUP_MENU_OPENED, OnPopupMenuOpened);
        Messenger.AddListener<PopupMenuBase>(Signals.POPUP_MENU_CLOSED, OnPopupMenuClosed);

        UpdateUI();
    }
    private void OnGameLoaded() {
        UpdateUI();
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
        UpdateFactionInfo();
        PlayerUI.Instance.UpdateUI();
    }
    private void UpdateInteractableInfoUI() {
        UpdateCharacterInfo();
        UpdateMinionInfo();
        UpdateTileObjectInfo();
        UpdateRegionInfo();
        UpdateQuestInfo();
        UpdateHextileInfo();
        UpdateStructureInfo();
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
    public void ToggleOptionsMenu() {
        if (_optionsMenu.isShowing) {
            _optionsMenu.Close();
        } else {
            _optionsMenu.Open();
        }
    }
    #endregion

    #region Minimap
    internal void UpdateMinimapInfo() {
        //CameraMove.Instance.UpdateMinimapTexture();
    }
    #endregion

    #region Tooltips
    public void ShowSmallInfo(string info, string header = "") {
        Profiler.BeginSample("Show Small Info Sample");
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";

        message = message.Replace("\\n", "\n");

        smallInfoLbl.text = message;
        if (!IsSmallInfoShowing()) {
            smallInfoGO.transform.SetParent(this.transform);
            smallInfoGO.SetActive(true);
        }
        PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
        Profiler.EndSample();
    }
    public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "") {
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";

        message = message.Replace("\\n", "\n");

        smallInfoLbl.text = message;
        if (!IsSmallInfoShowing()) {
            smallInfoGO.SetActive(true);
        }
        PositionTooltip(pos, smallInfoGO, smallInfoRT);
    }
    public void HideSmallInfo() {
        if (IsSmallInfoShowing()) {
            smallInfoGO.SetActive(false);
        }
    }
    public bool IsSmallInfoShowing() {
        return smallInfoGO.activeSelf;
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
    private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
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
    private void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt) {
        tooltipParent.transform.SetParent(position.transform);
        RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
        tooltipParentRT.pivot = position.pivot;

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        UtilityScripts.Utilities.GetAnchorMinMax(position.anchor, ref anchorMin, ref anchorMax);
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
    public bool IsSmallLocationInfoShowing() {
        return locationSmallInfoRT.gameObject.activeSelf;
    }
    public Region GetCurrentlyShowingSmallInfoLocation() {
        if (IsSmallLocationInfoShowing()) {
            return locationSmallInfo.region;
        }
        return null;
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
    private void OnUIMenuOpened(InfoUIBase menu) { }
    private void OnUIMenuClosed(InfoUIBase menu) { }
    private void OnPopupMenuOpened(PopupMenuBase menu) {
        openedPopups.Add(menu);
    }
    private void OnPopupMenuClosed(PopupMenuBase menu) {
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

        return _raycastResults.Count > 0 && _raycastResults.Any(go => go.gameObject.layer == LayerMask.NameToLayer("UI") || go.gameObject.layer == LayerMask.NameToLayer("WorldUI") || go.gameObject.layer == LayerMask.NameToLayer("Map_Click_Blocker"));
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
        string identifier = "", bool showCover = false, int layer = 9, bool closable = true, Func<string,Sprite> portraitGetter = null, bool shouldConfirmOnPick = false, bool asButton = false) {

        objectPicker.ShowClickable(choices, onClickAction, comparer, validityChecker, title, onHoverAction,
            onHoverExitAction, identifier, showCover, layer, portraitGetter, shouldConfirmOnPick, asButton);
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
            if (areaCenterSprites[i].name.ToLower() == name.ToLower()) {
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
        factionInfoUI.SetData(faction);
        factionInfoUI.OpenMenu();
    }
    public void UpdateFactionInfo() {
        if (factionInfoUI.isShowing) {
            factionInfoUI.UpdateFactionInfo();
        }
    }
    #endregion

    #region Character Info
    [Space(10)]
    [Header("Character Info")]
    [SerializeField] internal CharacterInfoUI characterInfoUI;
    public void ShowCharacterInfo(Character character, bool centerOnCharacter = false) {
        if(character.minion != null) {
            ShowMinionInfo(character.minion, centerOnCharacter);
        } else {
            if (tempDisableShowInfoUI) {
                SetTempDisableShowInfoUI(false);
                return;
            }
            characterInfoUI.SetData(character);
            characterInfoUI.OpenMenu();
            if (centerOnCharacter) {
                character.CenterOnCharacter();
            }
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
        if (minionInfoUI.isShowing) {
            minionInfoUI.OnClickCloseMenu();
        }
    }
    #endregion

    #region Minion Info
    [Space(10)]
    [Header("Minion Info")]
    [SerializeField] internal MinionInfoUI minionInfoUI;
    private void ShowMinionInfo(Minion minion, bool centerOnCharacter = false) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        minionInfoUI.SetData(minion);
        minionInfoUI.OpenMenu();
        if (centerOnCharacter) {
            minion.character.CenterOnCharacter();
        }
    }
    private void UpdateMinionInfo() {
        if (minionInfoUI.isShowing) {
            minionInfoUI.UpdateMinionInfo();
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

    // #region Item Info
    // [Space(10)]
    // [Header("Item Object Info")]
    // [SerializeField] internal ItemInfoUI itemInfoUI;
    // public void ShowItemInfo(SpecialToken item) {
    //     if (tempDisableShowInfoUI) {
    //         SetTempDisableShowInfoUI(false);
    //         return;
    //     }
    //     itemInfoUI.SetData(item);
    //     itemInfoUI.OpenMenu();
    // }
    // public void UpdateItemInfo() {
    //     if (itemInfoUI.isShowing) {
    //         itemInfoUI.UpdateInfo();
    //     }
    // }
    // #endregion

    #region Quest Info
    [Space(10)]
    [Header("Quest UI")]
    public QuestInfoUI questInfoUI;
    public void ShowQuestInfo(Quest quest) {
        questInfoUI.ShowQuestInfoUI(quest);
    }
    public void UpdateQuestInfo() {
        if (questInfoUI.gameObject.activeSelf) {
            questInfoUI.UpdateQuestInfo();
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
            structureInfoUI.UpdateInfo();
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

    //#region Player
    //private void OnCombatDone(Combat combat) {
    //    ShowDeveloperNotification($"Combat at <b>{combat.location.name}</b>!", 5, () => ShowCombatLog(combat));
    //}
    //#endregion1

    #region Inner Map
    [Header("Inner Maps")]
    [SerializeField] private Button returnToWorldBtn;
    [SerializeField] private UIHoverPosition returnToWorldBtnTooltipPos;
    private void OnInnerMapOpened(Region location) {
        worldUIRaycaster.enabled = false;
    }
    private void OnInnerMapClosed(Region location) {
        worldUIRaycaster.enabled = true;
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
    public void OpenShareIntelMenu(Character targetCharacter) {
        shareIntelMenu.Open(targetCharacter);
    }
    public void OpenShareIntelMenu(Character targetCharacter, Character actor, Intel intel) {
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
    [SerializeField] private RectTransform playerNotificationParent;
    [SerializeField] private GameObject intelPrefab;
    [SerializeField] private GameObject defaultNotificationPrefab;
    [SerializeField] private Button notifExpandButton;

    //public ScrollRect playerNotifScrollView;
    public GameObject playerNotifGO;
    public RectTransform playerNotificationScrollRectTransform;
    public ScrollRect playerNotifScrollRect;
    public Image[] playerNotifTransparentImages;
    public int maxPlayerNotif;
    public List<PlayerNotificationItem> activeNotifications = new List<PlayerNotificationItem>(); //notifications that are currently being shown.
    private void ShowPlayerNotification(Intel intel) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        IntelNotificationItem newItem = newIntelGO.GetComponent<IntelNotificationItem>();
        newItem.Initialize(intel, true, OnNotificationDestroyed);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem);
    }
    private void ShowPlayerNotification(Log log) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, true, OnNotificationDestroyed);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem);        
    }
    public void ShowPlayerNotification(Log log, int tick) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, true, OnNotificationDestroyed);
        newItem.SetTickShown(tick);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem);
    }
    private void PlaceNewNotification(PlayerNotificationItem newNotif) {
        //check if the log used is from a GoapAction
        //then check all other currently showing notifications, if it is from the same goap action and the active character of both logs are the same.
        //replace that log with this new one
        PlayerNotificationItem itemToReplace = null;
        if (newNotif.shownLog != null && newNotif.shownLog.category == "GoapAction") {
            for (int i = 0; i < activeNotifications.Count; i++) {
                PlayerNotificationItem currItem = activeNotifications[i];
                if (currItem.shownLog.category == "GoapAction" && currItem.shownLog.file == newNotif.shownLog.file
                    && currItem.shownLog.HasFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER) && newNotif.shownLog.HasFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER)
                    && currItem.shownLog.GetFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER).obj == newNotif.shownLog.GetFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER).obj) {
                    itemToReplace = currItem;
                    break;
                }
            }
        }

        if (itemToReplace != null) {
            newNotif.SetTickShown(itemToReplace.tickShown);
            int index = (itemToReplace.transform as RectTransform).GetSiblingIndex();
            itemToReplace.DeleteNotification();
            (newNotif.gameObject.transform as RectTransform).SetSiblingIndex(index);
        }
        //else {
        //    (newNotif.gameObject.transform as RectTransform).SetAsLastSibling();
        //}
        activeNotifications.Add(newNotif);
        if (!notifExpandButton.gameObject.activeSelf) {
            //notifExpandButton.gameObject.SetActive(true);
        }
        if (activeNotifications.Count > maxPlayerNotif) {
            activeNotifications[0].DeleteNotification();
        }
    }
    private void OnNotificationDestroyed(PlayerNotificationItem item) {
        activeNotifications.Remove(item);
        if(activeNotifications.Count <= 0) {
            //notifExpandButton.gameObject.SetActive(false);
        }
    }
    public void OnClickExpand() {
        if(playerNotificationScrollRectTransform.sizeDelta.y == 950f) {
            playerNotificationScrollRectTransform.sizeDelta = new Vector2(playerNotificationScrollRectTransform.sizeDelta.x, 194f);
        }else if (playerNotificationScrollRectTransform.sizeDelta.y == 194f) {
            playerNotificationScrollRectTransform.sizeDelta = new Vector2(playerNotificationScrollRectTransform.sizeDelta.x, 950f);
        }
        //Canvas.ForceUpdateCanvases();
    }
    public void OnHoverNotificationArea() {
        for (int i = 0; i < playerNotifTransparentImages.Length; i++) {
            playerNotifTransparentImages[i].color = new Color(playerNotifTransparentImages[i].color.r, playerNotifTransparentImages[i].color.g, playerNotifTransparentImages[i].color.b, 120f/255f);
        }
    }
    public void OnHoverExitNotificationArea() {
        for (int i = 0; i < playerNotifTransparentImages.Length; i++) {
            playerNotifTransparentImages[i].color = new Color(playerNotifTransparentImages[i].color.r, playerNotifTransparentImages[i].color.g, playerNotifTransparentImages[i].color.b, 25f/255f);
        }
    }
    public void ShowPlayerNotificationArea() {
        UtilityScripts.Utilities.DestroyChildren(playerNotifScrollRect.content);
        playerNotifGO.SetActive(true);
    }
    public void HidePlayerNotificationArea() {
        playerNotifGO.SetActive(false);
    }
    private void MoveNotificationMenuToDefaultPos() {
        playerNotificationParent.anchoredPosition = new Vector2(506f, 170f);
    }
    private void MoveNotificationMenuToModifiedPos() {
        playerNotificationParent.anchoredPosition = new Vector2(930f, 170f);
    }
    #endregion

    #region Interior Map Loading
    [Header("Interior Map Loading")]
    [SerializeField] private GameObject interiorMapLoadingGO;
    [SerializeField] private Image interiorMapLoadingBGImage;
    [SerializeField] private GameObject interiorMapLoadingDetailsGO;
    public void SetInteriorMapLoadingState(bool state) {
        interiorMapLoadingGO.SetActive(state);
        //if (state) {
        //    interiorMapLoadingGO.SetActive(true);
        //    //StartCoroutine(LoadingCoroutine(128f/ 255f));
        //    //interiorMapLoadingTween.PlayForward();
        //} else {
        //    //interiorMapLoadingTween.PlayReverse();
        //    //interiorMapLoadingBGImage.CrossFadeAlpha(0, 0.5f, true);
        //    //StartCoroutine(LoadingCoroutine(0f / 255f));
        //}
    }
    #endregion

    #region Yes/No
    [Header("Yes or No Confirmation")]
    [SerializeField] private GameObject yesNoGO;
    [SerializeField] private GameObject yesNoCover;
    [SerializeField] private TextMeshProUGUI yesNoHeaderLbl;
    [SerializeField] private TextMeshProUGUI yesNoDescriptionLbl;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI yesBtnLbl;
    [SerializeField] private TextMeshProUGUI noBtnLbl;
    [SerializeField] private HoverHandler yesBtnUnInteractableHoverHandler;
    public void ShowYesNoConfirmation(string header, string question, System.Action onClickYesAction = null, System.Action onClickNoAction = null,
        bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, bool pauseAndResume = false, 
        bool yesBtnActive = true, bool noBtnActive = true, System.Action yesBtnInactiveHoverAction = null, System.Action yesBtnInactiveHoverExitAction = null) {
        if (pauseAndResume) {
            SetSpeedTogglesState(false);
            Pause();
        }
        yesNoHeaderLbl.text = header;
        yesNoDescriptionLbl.text = question;

        yesBtnLbl.text = yesBtnText;
        noBtnLbl.text = noBtnText;

        yesBtn.gameObject.SetActive(yesBtnActive);
        noBtn.gameObject.SetActive(noBtnActive);

        yesBtn.interactable = yesBtnInteractable;
        noBtn.interactable = noBtnInteractable;

        //clear all listeners
        yesBtn.onClick.RemoveAllListeners();
        noBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();

        //hide confirmation menu on click
        yesBtn.onClick.AddListener(HideYesNoConfirmation);
        noBtn.onClick.AddListener(HideYesNoConfirmation);
        closeBtn.onClick.AddListener(HideYesNoConfirmation);

        //resume last prog speed on click any btn
        if (pauseAndResume) {
            yesBtn.onClick.AddListener(ResumeLastProgressionSpeed);
            noBtn.onClick.AddListener(ResumeLastProgressionSpeed);
            closeBtn.onClick.AddListener(ResumeLastProgressionSpeed);
        }

        //specific actions
        if (onClickYesAction != null) {
            yesBtn.onClick.AddListener(onClickYesAction.Invoke);
        }
        if (onClickNoAction != null) {
            noBtn.onClick.AddListener(onClickNoAction.Invoke);
            //closeBtn.onClick.AddListener(onClickNoAction.Invoke);
        }

        yesBtnUnInteractableHoverHandler.gameObject.SetActive(!yesBtn.interactable);
        if (yesBtnInactiveHoverAction != null) {
            yesBtnUnInteractableHoverHandler.SetOnHoverAction(yesBtnInactiveHoverAction.Invoke);
        }
        if (yesBtnInactiveHoverExitAction != null) {
            yesBtnUnInteractableHoverHandler.SetOnHoverOutAction(yesBtnInactiveHoverExitAction.Invoke);
        }

        yesNoGO.SetActive(true);
        yesNoGO.transform.SetSiblingIndex(layer);
        yesNoCover.SetActive(showCover);
    }
    private void HideYesNoConfirmation() {
        yesNoGO.SetActive(false);
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

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        UtilityScripts.Utilities.GetAnchorMinMax(position.anchor, ref anchorMin, ref anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;
        tooltipParentRT.anchoredPosition = Vector2.zero;
    }
    #endregion
}