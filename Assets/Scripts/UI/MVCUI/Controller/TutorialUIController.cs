using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using Ruinarch.MVCFramework;
using Tutorial;
using UnityEngine;
using UtilityScripts;

public class TutorialUIController : MVCUIController, TutorialUIView.IListener {
    [SerializeField]
    private TutorialUIModel m_tutorialUIModel;
    private TutorialUIView m_tutorialUIView;

    private List<TutorialItemUI> _items;

    public bool isShowing { get; private set; }
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        TutorialUIView.Create(_canvas, m_tutorialUIModel, (p_ui) => {
            m_tutorialUIView = p_ui;
            m_tutorialUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
        });
    }
    private void Awake() {
        _items = new List<TutorialItemUI>();
        TutorialItemUI.onTutorialItemToggledOn = OnTutorialItemSelected;
        TutorialItemUI.onTutorialItemToggledOff = OnTutorialItemDeselected;
    }
    private void Start() {
        InstantiateUI();
        HideUI();
        m_tutorialUIView.UIModel.goTutorialPages.SetActive(false);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
    }
    public void OnGameLoaded() {
        CreateInitialItems();
    }
    private void CreateInitialItems() {
        if (SaveManager.Instance == null) {
            TutorialManager.Tutorial_Type[] tutorialTypes = CollectionUtilities.GetEnumValues<TutorialManager.Tutorial_Type>();
            for (int i = 0; i < tutorialTypes.Length; i++) {
                TutorialManager.Tutorial_Type tutorialType = tutorialTypes[i];
                CreateTutorialItem(tutorialType);
            }    
        } else {
            List<TutorialManager.Tutorial_Type> unlockedTutorials = SaveManager.Instance.currentSaveDataPlayer.unlockedTutorials.OrderBy(t => t.GetTutorialOrder()).ToList();
            for (int i = 0; i < unlockedTutorials.Count; i++) {
                TutorialManager.Tutorial_Type tutorialType = unlockedTutorials[i];
                CreateTutorialItem(tutorialType);
            }    
        }
        
    }

    private void CreateTutorialItem(TutorialManager.Tutorial_Type p_type) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialItemUI", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.scrollRectTutorialItems.content);
        TutorialItemUI itemUI = go.GetComponent<TutorialItemUI>();
        itemUI.Initialize(p_type, m_tutorialUIView.UIModel.toggleGroupTutorialItems);
        _items.Add(itemUI);
    }
    private void OnTutorialItemSelected(TutorialManager.Tutorial_Type p_type) {
        m_tutorialUIView.UIModel.goTutorialPages.SetActive(true);
        //Load pages based on tutorial type
        LoadPagesForTutorial(p_type);
        m_tutorialUIView.UIModel.tutorialPagesScrollSnap.GoToScreen(0);
        if (SaveManager.Instance != null) {
            SaveManager.Instance.savePlayerManager.currentSaveDataPlayer.SetTutorialAsRead(p_type);
            SaveManager.Instance.savePlayerManager.SavePlayerData();
        }
    }
    private void OnTutorialItemDeselected(TutorialManager.Tutorial_Type p_type) {
        m_tutorialUIView.UIModel.goTutorialPages.SetActive(false);
    }
    private void LoadPagesForTutorial(TutorialManager.Tutorial_Type p_type) {
        UtilityScripts.Utilities.DestroyChildrenObjectPool(m_tutorialUIView.UIModel.tutorialPagesScrollSnap._scroll_rect.content);
        UtilityScripts.Utilities.DestroyChildren(m_tutorialUIView.UIModel.tutorialPaginationParent);
        TutorialScriptableObjectData data = TutorialManager.Instance.GetTutorialData(p_type);
        for (int i = 0; i < data.pages.Count; i++) {
            TutorialPage page = data.pages[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialPageItem", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.tutorialPagesScrollSnap._scroll_rect.content);
            TutorialPageItem item = go.GetComponent<TutorialPageItem>();
            item.Initialize(page);
            ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialPagination", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.tutorialPaginationParent);
            m_tutorialUIView.UIModel.tutorialPagesScrollSnap.UpdateChildrenAndPagination();
        }
    }
    public void OnClickClose() {
        HideUI();
    }
    public override void HideUI() {
        for (int i = 0; i < _items.Count; i++) {
            TutorialItemUI itemUI = _items[i];
            if (itemUI.toggle.isOn) {
                itemUI.toggle.isOn = false;
            }
        }
        m_tutorialUIView.UIModel.goTutorialPages.SetActive(false);
        UtilityScripts.Utilities.DestroyChildren(m_tutorialUIView.UIModel.tutorialPaginationParent);
        base.HideUI();
        isShowing = false;
        if (PlayerUI.Instance != null) {
            PlayerUI.Instance.OnCloseTutorialUI();    
        }
        if (TutorialManager.Instance != null) {
            TutorialManager.Instance.UnloadTutorialAssets();    
        }
        UIManager.Instance.ResumeLastProgressionSpeed();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
        InnerMapCameraMove.Instance.EnableMovement();
    }
    public override void ShowUI() {
        base.ShowUI();
        isShowing = true;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        InputManager.Instance.SetSpecificHotkeyEnabledState(KeyCode.Escape, true);
        InnerMapCameraMove.Instance.DisableMovement();
        m_tutorialUIView.UIModel.scrollRectTutorialItems.verticalNormalizedPosition = 1f;
    }
    public void HideViaShortcutKey() {
        HideUI();
    }
    public void JumpToSpecificTutorial(TutorialManager.Tutorial_Type p_type) {
        TutorialItemUI tutorialItemUI = GetTutorialItem(p_type);
        if (tutorialItemUI != null) {
            tutorialItemUI.ManualSelect();
        }
    }
    private TutorialItemUI GetTutorialItem(TutorialManager.Tutorial_Type p_type) {
        for (int i = 0; i < _items.Count; i++) {
            TutorialItemUI tutorialItemUI = _items[i];
            if (tutorialItemUI.tutorialType == p_type) {
                return tutorialItemUI;
            }
        }
        return null;
    }
}
