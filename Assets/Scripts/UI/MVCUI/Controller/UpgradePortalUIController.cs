using Ruinarch.MVCFramework;
using UnityEngine;

public class UpgradePortalUIController : MVCUIController, UpgradePortalUIView.IListener {
    [SerializeField]
    private UpgradePortalUIModel m_portalUIModel;
    private UpgradePortalUIView m_portalUIView;
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        UpgradePortalUIView.Create(_canvas, m_portalUIModel, (p_ui) => {
            m_portalUIView = p_ui;
            m_portalUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
        });
    }

    #region UpgradePortalUIView.IListener
    public void OnClickClose() {
        HideUI();
    }
    public void OnClickUpgrade() {
        throw new System.NotImplementedException();
    }
    #endregion
    
}
