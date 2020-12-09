using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIController : MVCUIController, BlackmailUIView.IListener {
    [SerializeField] private BlackmailUIModel m_blackmailUIModel;
    private BlackmailUIView m_blackmailUIView;
    private void Start() {
        InstantiateUI();
    }
    private void OnEnable() {
        BlackmailUIItem.onChooseBlackmail += OnChooseIntel;
    }
    private void OnDisable() {
        BlackmailUIItem.onChooseBlackmail -= OnChooseIntel;
    }

    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        BlackmailUIView.Create(_canvas, m_blackmailUIModel, (p_ui) => {
            m_blackmailUIView = p_ui;
            m_blackmailUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
        });
    }

    private void OnChooseIntel(IIntel p_intel) {
        
    }

    #region BlackmailUIView.IListener Implementation
    public void OnClickClose() {
        HideUI();
    }
    #endregion
}
