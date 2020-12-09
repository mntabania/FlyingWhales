using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIController : MVCUIController, BlackmailUIView.IListener {
    [SerializeField] private BlackmailUIModel m_blackmailUIModel;
    private BlackmailUIView m_blackmailUIView;

    private IIntel _chosenBlackmail;
    private System.Action<IIntel> _onConfirmAction;
    
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

    public void Show(List<IIntel> p_blackmail, System.Action<IIntel> p_onConfirmAction) {
        ShowUI();
        m_blackmailUIView.DisplayBlackmailItems(p_blackmail);
        _onConfirmAction = p_onConfirmAction;
    }

    private void OnChooseIntel(IIntel p_intel, bool p_isOn) {
        if (p_isOn) {
            _chosenBlackmail = p_intel;
            Debug.Log($"Chosen intel {p_intel?.log.logText}");    
        }
    }

    #region BlackmailUIView.IListener Implementation
    public void OnClickClose() {
        HideUI();
    }
    public void OnClickConfirm() {
        HideUI();
        _onConfirmAction?.Invoke(_chosenBlackmail);
    }
    #endregion
}
