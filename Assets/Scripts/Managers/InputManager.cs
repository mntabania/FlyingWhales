using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Ruinarch.Custom_UI;
using Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Locations.Settlements;
using UtilityScripts;

namespace Ruinarch {
    public class InputManager : MonoBehaviour {

        public static InputManager Instance;

        public enum Cursor_Type {
            None, Default, Target, Drag_Hover, Drag_Clicked, Check, Cross, Link
        }

        #region Events
        private static System.Action m_onUpdateEvent;
        #endregion
        
        [Space(10)] 
        [Header("Cursors")] 
        [SerializeField] private CursorTextureDictionary cursors;
        
        public HashSet<string> buttonsToHighlight { get; private set; }
        public Cursor_Type currentCursorType;
        public Cursor_Type previousCursorType;
        public bool isDraggingItem;
        
        private CursorMode cursorMode = CursorMode.ForceSoftware;
        private bool runUpdate;
        private bool _isShiftDown;

        private Dictionary<KeyCode, bool> _allowedHotKeys;
        private List<KeyCode> _allowedHotKeysList = new List<KeyCode>() {
            {KeyCode.Alpha1},
            {KeyCode.Alpha2},
            {KeyCode.Alpha3},
            {KeyCode.Alpha4},
            {KeyCode.Alpha5},
            {KeyCode.Alpha6},
            {KeyCode.Alpha7},
            {KeyCode.Alpha8},
            {KeyCode.Alpha9},
            {KeyCode.F10},
            {KeyCode.Space},
            {KeyCode.Escape},
            {KeyCode.F5},
            {KeyCode.F8},
            {KeyCode.Tab},
            {KeyCode.LeftAlt},
            {KeyCode.LeftShift},
            {KeyCode.P},
            {KeyCode.Minus},
            {KeyCode.Plus},
            {KeyCode.Equals},
            {KeyCode.KeypadPlus},
            {KeyCode.KeypadMinus},
        };

        #region getters
        public bool isShiftDown => _isShiftDown;
        #endregion
        
        #region Monobehaviours
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetCursorTo(Cursor_Type.Default);
                previousCursorType = Cursor_Type.Default;
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                Initialize();
            } else {
                Destroy(gameObject);
            }
        }
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance == null) {
                if (SettingsManager.Instance.IsShowing()) {
                    SettingsManager.Instance.CloseSettings();
                    return;
                }
            } else if (Input.GetKeyDown(KeyCode.F10)) {
                if (!CanUseHotkey(KeyCode.F10)) return;
                ReportABug();
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.F10);
            }
            
            if (runUpdate == false) { return; }
            m_onUpdateEvent?.Invoke();
            if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive()) {
                //Do not allow any hotkeys while loading
                return;
            }
            if (Input.GetMouseButtonDown(0)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Mouse0);
            } else if (Input.GetMouseButtonDown(1)) {
                if (!EventSystem.current.IsPointerOverGameObject()) {
                    Messenger.Broadcast(ControlsSignals.KEY_DOWN_EMPTY_SPACE, KeyCode.Mouse1);
                }
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Mouse1);
                CancelSpellsByPriority();
            } else if (Input.GetMouseButtonDown(2)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Mouse2);
            } else if (Input.GetKeyDown(KeyCode.BackQuote)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.BackQuote);
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                if (!CanUseHotkey(KeyCode.Space)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Space);
            } else if (Input.GetKeyDown(KeyCode.Minus)) {
                if (!CanUseHotkey(KeyCode.Minus)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Minus);
            } else if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                if (!CanUseHotkey(KeyCode.KeypadMinus)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.KeypadMinus);
            } else if (Input.GetKeyDown(KeyCode.Plus)) {
                if (!CanUseHotkey(KeyCode.Plus)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Plus);
            } else if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                if (!CanUseHotkey(KeyCode.KeypadPlus)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.KeypadPlus);
            } else if (Input.GetKeyDown(KeyCode.Equals)) {
                if (!CanUseHotkey(KeyCode.Equals)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Equals);
            } else if (Input.GetKeyDown(KeyCode.F5)) {
                if (!CanUseHotkey(KeyCode.F5)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.F5);
            } else if (Input.GetKeyDown(KeyCode.F8)) {
                if (!CanUseHotkey(KeyCode.F8)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.F8);
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                if (!CanUseHotkey(KeyCode.Escape)) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Escape);
                if (UIManager.Instance != null) {
                    if (!CancelActionsByPriority(true)) {
                        //if no actions were cancelled then show options menu if it is not yet showing.
                        //if game has started then, check if options menu is not showing, if it is not, then
                        //show options menu, then do not cancel any actions.
                        if (!UIManager.Instance.IsOptionsMenuShowing()) {
                            UIManager.Instance.OpenOptionsMenu();
                            return;
                        }    
                    }
                }
            } else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                BroadcastHotkeyPress("Spells Tab", KeyCode.Alpha1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                BroadcastHotkeyPress("Structures Tab", KeyCode.Alpha2);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                BroadcastHotkeyPress("Demons Tab", KeyCode.Alpha3);
            } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                BroadcastHotkeyPress("Monsters Tab", KeyCode.Alpha4);
            } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
                BroadcastHotkeyPress("Intel Tab", KeyCode.Alpha5);
            } else if (Input.GetKeyDown(KeyCode.Alpha6)) {
                BroadcastHotkeyPress("Targets Tab", KeyCode.Alpha6);
            } else if (Input.GetKeyDown(KeyCode.Alpha7)) {
                BroadcastHotkeyPress("Villagers Tab", KeyCode.Alpha7);
            } else if (Input.GetKeyDown(KeyCode.Alpha8)) {
                BroadcastHotkeyPress("Cultist Tab", KeyCode.Alpha8);
            } else if (Input.GetKeyDown(KeyCode.Alpha9)) {
                BroadcastHotkeyPress("Tutorials Tab", KeyCode.Alpha9);
            } else if (Input.GetKeyDown(KeyCode.P)) {
                BroadcastHotkeyPress("portal shortcut", KeyCode.P);
            } else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (!CanUseHotkey(KeyCode.Tab)) return;
                if (HasSelectedUIObject()) { return; } //if currently selecting a UI object, ignore (This is mostly for Input fields)
                CharacterCenterCycle();
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Tab);
            } else if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                if (!CanUseHotkey(KeyCode.LeftAlt)) return;
                if (GameManager.Instance != null && GameManager.Instance.gameHasStarted) {
                    CharacterManager.Instance.ToggleCharacterMarkerNameplate();
                }
            } else if (Input.GetKeyDown(KeyCode.LeftShift)) {
                if (!CanUseHotkey(KeyCode.LeftShift)) return;
                _isShiftDown = true;
                Messenger.Broadcast(ControlsSignals.LEFT_SHIFT_DOWN);
            }
            
            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                _isShiftDown = false;
                Messenger.Broadcast(ControlsSignals.LEFT_SHIFT_UP);
            }

            
        }
        private void BroadcastHotkeyPress(string buttonToActivate, KeyCode p_keyCode) {
            if (!CanUseHotkey(p_keyCode)) return;
            if (HasSelectedUIObject()) { return; } //if currently selecting a UI object, ignore (This is mostly for Input fields)
            Messenger.Broadcast(UISignals.HOTKEY_CLICK, buttonToActivate);
        }
        private bool CanUseHotkey(KeyCode p_keyCode) {
            if (SaveManager.Instance.saveCurrentProgressManager.isSaving) {
                //Do not allow hotkeys while saving
                return false;
            }
            if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive()) {
                //Do not allow hotkeys while loading
                return false;
            }
            if (p_keyCode != KeyCode.Escape) {
                //Allow escape if any popup is showing 
                if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing()) {
                    return false;
                }
                if (UIManager.Instance != null && UIManager.Instance.IsObjectPickerOpen()) {
                    return false;
                }    
            }
            return _allowedHotKeys[p_keyCode];
        }
        public void SetAllHotkeysEnabledState(bool p_state) {
#if DEBUG_LOG
            Debug.Log($"Set all hotkeys state to {p_state.ToString()}");
#endif
            List<KeyCode> keys = _allowedHotKeysList;
            for (int i = 0; i < keys.Count; i++) {
                KeyCode key = keys[i];
                SetSpecificHotkeyEnabledState(key, p_state);
            }
        }
        public void SetSpecificHotkeyEnabledState(KeyCode p_keyCode, bool p_state) {
            _allowedHotKeys[p_keyCode] = p_state;
// #if DEBUG_LOG
//             Debug.Log($"Set hotkeys state of {p_keyCode} to {p_state}");
// #endif
        }
#endregion

#region Initialization
        private void Initialize() {
            buttonsToHighlight = new HashSet<string>();
            Messenger.MarkAsPermanent(UISignals.SHOW_SELECTABLE_GLOW);
            Messenger.MarkAsPermanent(UISignals.HIDE_SELECTABLE_GLOW);
            Messenger.MarkAsPermanent(UISignals.TOGGLE_SHOWN);
            Messenger.AddListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveHighlightSignal);
            Messenger.AddListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveUnHighlightSignal);
            Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_SHOWN, OnToggleShown);
            Messenger.AddListener<RuinarchButton>(UISignals.BUTTON_SHOWN, OnButtonShown);

            ConstructHotKeys();
        }
        private void ConstructHotKeys() {
            _allowedHotKeys = new Dictionary<KeyCode, bool>();
            for (int i = 0; i < _allowedHotKeysList.Count; i++) {
                KeyCode hotKey = _allowedHotKeysList[i];
                _allowedHotKeys.Add(hotKey, true);
            }
        }
        private void OnReceiveHighlightSignal(string name) {
            buttonsToHighlight.Add(name);
        }
        private void OnReceiveUnHighlightSignal(string name) {
            buttonsToHighlight.Remove(name);
        }
        private void OnToggleShown(RuinarchToggle toggle) {
            if (buttonsToHighlight.Contains(toggle.name)) {
                toggle.StartGlow();
            }
        }
        private void OnButtonShown(RuinarchButton button) {
            if (buttonsToHighlight.Contains(button.name)) {
                button.StartGlow();
            }
        }
#endregion

#region Cursor
        public void SetCursorTo(Cursor_Type type) {
            if (currentCursorType == type) {
                return; //ignore 
            }
            previousCursorType = currentCursorType;
            Vector2 hotSpot = Vector2.zero;
            switch (type) {
                case Cursor_Type.Drag_Clicked:
                    isDraggingItem = true;
                    break;
                case Cursor_Type.Check:
                case Cursor_Type.Cross:
                case Cursor_Type.Link:
                    hotSpot = new Vector2(12f, 10f);
                    break;
                case Cursor_Type.Target:
                    hotSpot = new Vector2(29f, 29f);
                    break;
                default:
                    isDraggingItem = false;
                    break;
            }
            currentCursorType = type;
            Cursor.SetCursor(cursors[type], hotSpot, cursorMode);
#if DEBUG_LOG
            Debug.Log($"Set cursor to {currentCursorType.ToString()}");
#endif
        }
        public void RevertToPreviousCursor() {
            SetCursorTo(previousCursorType);
        }
#endregion

#region Spells
        /// <summary>
        /// Cancel actions based on a hardcoded process
        /// </summary>
        private bool CancelActionsByPriority(bool ignoreCursor = false) {
            if (SettingsManager.Instance.IsShowing()) {
                SettingsManager.Instance.CloseSettings();
                return true;
            }
            if (UIManager.Instance == null) {
                return true;
            }
            if (SaveManager.Instance != null && SaveManager.Instance.saveCurrentProgressManager.isSaving) {
                return true;
            }
            // if (PlayerManager.Instance == null) {
            //     return true;
            // }
            UIManager.Instance.SetTempDisableShowInfoUI(false);
            if (!CancelSpellsByPriority()) {
                if (UIManager.Instance.IsOptionsMenuShowing()) {
                    //if options menu is showing, check if load window is showing, if it is close load window.
                    if (UIManager.Instance.IsLoadWindowShowing()) {
                        UIManager.Instance.CloseLoadWindow();
                        return true;
                    }
                    //if load window is not showing then close options menu
                    UIManager.Instance.CloseOptionsMenu();
                    return true;
                }
                if (UIManager.Instance.IsContextMenuShowing()) {
                    UIManager.Instance.HidePlayerActionContextMenu();
                    return true;
                }
                if (UIManager.Instance.biolabUIController.isShowing) {
                    UIManager.Instance.biolabUIController.HideViaShortcutKey();
                    return true;
                }
                if (UIManager.Instance.upgradePortalUIController.isShowing) {
                    //TODO: Improve this
                    if (UIManager.Instance.yesNoConfirmation.isShowing) {
                        UIManager.Instance.yesNoConfirmation.Close();
                        return true;
                    }
                    UIManager.Instance.upgradePortalUIController.HideViaShortcutKey();
                    return true;
                }
                if (UIManager.Instance.purchaseSkillUIController.isShowing) {
                    //TODO: Improve this
                    if (UIManager.Instance.yesNoConfirmation.isShowing) {
                        UIManager.Instance.yesNoConfirmation.Close();
                        return true;
                    }
                    UIManager.Instance.purchaseSkillUIController.HideViaShortcutKey();
                    return true;
                }
                if (PlayerUI.Instance.tutorialUIController.isShowing) {
                    PlayerUI.Instance.tutorialUIController.HideViaShortcutKey();
                    return true;
                }
                CustomStandaloneInputModule customModule = EventSystem.current.currentInputModule as CustomStandaloneInputModule;
                if (ignoreCursor || !EventSystem.current.IsPointerOverGameObject() || customModule.GetPointerData().pointerEnter.GetComponent<Button>() == null) {
                    if (UIManager.Instance.openedPopups.Count > 0) {
                        //close latest popup
                        UIManager.Instance.openedPopups.Last().Close();
                        return true;
                    } else {
                        if (UIManager.Instance.poiTestingUI.gameObject.activeSelf) { //|| UIManager.Instance.minionCommandsUI.gameObject.activeSelf
                            return true;
                        }
                        //close latest Info UI
                        if (UIManager.Instance.latestOpenedInfoUI != null) {
                            //close latest popup
                            UIManager.Instance.latestOpenedInfoUI.OnClickCloseMenu();
                            return true;
                        }
                    }
                }
                return false;
            } else {
                //cancelled a spell
                return true;
            }
        }
        private bool CancelSpellsByPriority() {
            if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActivePlayerSpell != null) {
                //cancel current spell
                PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
                return true;
            } else if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveIntel != null) {
                //cancel current intel
                PlayerManager.Instance.player.SetCurrentActiveIntel(null);
                return true;
            } else if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveItem != TILE_OBJECT_TYPE.NONE) {
                PlayerManager.Instance.player.SetCurrentlyActiveItem(TILE_OBJECT_TYPE.NONE);
                return true;
            } else if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveArtifact != ARTIFACT_TYPE.None) {
                PlayerManager.Instance.player.SetCurrentlyActiveArtifact(ARTIFACT_TYPE.None);
                return true;
            }
            return false;
        }
#endregion
        
#region Utilities
        private void OnActiveSceneChanged(Scene current, Scene next) {
            if (next.name == "Game") {
                runUpdate = true;
            } else {
                runUpdate = false;
            }
            buttonsToHighlight.Clear();
        }
        public bool ShouldBeHighlighted(RuinarchButton button) {
            return buttonsToHighlight.Contains(button.name);
        }
        public bool ShouldBeHighlighted(RuinarchToggle button) {
            return buttonsToHighlight.Contains(button.name);
        }
        public bool HasSelectedUIObject() {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            return currentSelectedGameObject != null && currentSelectedGameObject.activeInHierarchy;
        }
#endregion

#region Selection
        public void Select(ISelectable objToSelect) {
            objToSelect.LeftSelectAction();
            Messenger.Broadcast(ControlsSignals.SELECTABLE_LEFT_CLICKED, objToSelect);
        }
#endregion

#region Report A Bug
        private void ReportABug() {
            YesNoConfirmation yesNoConfirmation = null;
            if (UIManager.Instance != null) {
                yesNoConfirmation = UIManager.Instance.yesNoConfirmation;
            } else if (MainMenuUI.Instance != null) {
                yesNoConfirmation = MainMenuUI.Instance.yesNoConfirmation;
            }
            if (yesNoConfirmation != null) {
                if (!yesNoConfirmation.isShowing) {
                    yesNoConfirmation.ShowYesNoConfirmation("Open Browser", "To report a bug, the game needs to open a Web browser, do you want to proceed?",
                        () => Application.OpenURL("https://forms.gle/gcoa8oHxywFLegNx7"), layer: 50, showCover: true);    
                }
            }
        }
#endregion

#region Events
        public static void AddOnUpdateEvent(System.Action p_event) {
            m_onUpdateEvent += p_event;
        }
        public static void RemoveOnUpdateEvent(System.Action p_event) {
            m_onUpdateEvent -= p_event;
        } 
#endregion
        
#region Center Cycle
        private void CharacterCenterCycle() {
            if (DatabaseManager.Instance.characterDatabase.aliveVillagersList != null && DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count > 0) {
                //normal objects to center
                ISelectable objToSelect = GetNextCharacterToCenter(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
                if (objToSelect != null) {
                    Select(objToSelect);
                }
            }
        }
        private Character GetNextCharacterToCenter(List<Character> selectables) {
            Character objToSelect = null;
            for (int i = 0; i < selectables.Count; i++) {
                Character currentSelectable = selectables[i];
                if (currentSelectable.IsCurrentlySelected()) {
                    //set next selectable in list to be selected.
                    objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                    break;
                }
            }
            if (objToSelect == null) {
                objToSelect = selectables[0];
            }
            return objToSelect;
        }
#endregion
    }
}