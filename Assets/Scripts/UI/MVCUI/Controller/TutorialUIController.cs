using Ruinarch.MVCFramework;
using UnityEngine;

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
}
