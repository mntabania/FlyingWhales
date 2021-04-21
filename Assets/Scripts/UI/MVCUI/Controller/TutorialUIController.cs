using System;
using Ruinarch.MVCFramework;
using Tutorial;
using UnityEngine;
using UtilityScripts;

public class TutorialUIController : MVCUIController, TutorialUIView.IListener {
    [SerializeField]
    private TutorialUIModel m_tutorialUIModel;
    private TutorialUIView m_tutorialUIView;
    
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
        TutorialItemUI.onClickTutorialItem = OnClickTutorialItem;
        InstantiateUI();
        HideUI();
    }
    private void Start() {
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
            for (int i = 0; i < SaveManager.Instance.currentSaveDataPlayer.unlockedTutorials.Count; i++) {
                TutorialManager.Tutorial_Type tutorialType = SaveManager.Instance.currentSaveDataPlayer.unlockedTutorials[i];
                CreateTutorialItem(tutorialType);
            }    
        }
        
    }

    private void CreateTutorialItem(TutorialManager.Tutorial_Type p_type) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialItemUI", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.scrollRectTutorialItems.content);
        TutorialItemUI itemUI = go.GetComponent<TutorialItemUI>();
        itemUI.Initialize(p_type, m_tutorialUIView.UIModel.toggleGroupTutorialItems);
    }
    private void OnClickTutorialItem(TutorialManager.Tutorial_Type p_type) {
        //Load pages based on tutorial type
        LoadPagesForTutorial(p_type);
    }
    private void LoadPagesForTutorial(TutorialManager.Tutorial_Type p_type) {
        UtilityScripts.Utilities.DestroyChildrenObjectPool(m_tutorialUIView.UIModel.tutorialPagesScrollSnap._scroll_rect.content);
        TutorialScriptableObjectData data = TutorialManager.Instance.GetTutorialData(p_type);
        for (int i = 0; i < data.pages.Count; i++) {
            TutorialPage page = data.pages[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialPageItem", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.tutorialPagesScrollSnap._scroll_rect.content);
            TutorialPageItem item = go.GetComponent<TutorialPageItem>();
            item.Initialize(page);
        }
    }
    public void OnClickClose() {
        HideUI();
        if (PlayerUI.Instance != null) {
            PlayerUI.Instance.OnCloseTutorialUI();    
        }
    }
}
