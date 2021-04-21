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

    private void CreateInitialItems() {
        for (int i = 0; i < SaveManager.Instance.currentSaveDataPlayer.unlockedTutorials.Count; i++) {
            TutorialManager.Tutorial_Type tutorialType = SaveManager.Instance.currentSaveDataPlayer.unlockedTutorials[i];
            CreateTutorialItem(tutorialType);
        }
    }

    private void CreateTutorialItem(TutorialManager.Tutorial_Type p_type) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialItemUI", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.scrollRectTutorialItems.content);
        TutorialItemUI itemUI = go.GetComponent<TutorialItemUI>();
        itemUI.Initialize(p_type);
    }
}
