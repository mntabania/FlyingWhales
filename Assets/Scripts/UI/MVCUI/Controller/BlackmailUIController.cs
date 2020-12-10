using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIController : MVCUIController, BlackmailUIView.IListener {
    [SerializeField] private BlackmailUIModel m_blackmailUIModel;
    private BlackmailUIView m_blackmailUIView;

    private List<IIntel> _chosenBlackmail;
    private System.Action<List<IIntel>> _onConfirmAction;
    private void OnEnable() {
        BlackmailUIItem.onChooseBlackmail += OnChooseIntel;
        BlackmailUIItem.onHoverOverBlackmail += OnHoverOverBlackmail;
        BlackmailUIItem.onHoverOutBlackmail += OnHoverOutBlackmail;
    }
    private void OnDisable() {
        BlackmailUIItem.onChooseBlackmail -= OnChooseIntel;
        BlackmailUIItem.onHoverOverBlackmail -= OnHoverOverBlackmail;
        BlackmailUIItem.onHoverOutBlackmail -= OnHoverOutBlackmail;
    }
    private void Awake() {
        _chosenBlackmail = new List<IIntel>();
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

    public void ShowBlackmailUI(List<IIntel> p_blackmail, List<IIntel> p_alreadyChosenBlackmail, Action<List<IIntel>> p_onConfirmAction) {
        ShowUI();
        _chosenBlackmail.Clear();
        m_blackmailUIView.DisplayBlackmailItems(p_blackmail, p_alreadyChosenBlackmail);
        _onConfirmAction = p_onConfirmAction;
    }

    private void OnChooseIntel(IIntel p_intel, bool p_isOn) {
        if (p_isOn) {
            _chosenBlackmail.Add(p_intel);
            Debug.Log($"Chosen intel {p_intel?.log.logText}");    
        } else {
            _chosenBlackmail.Remove(p_intel);
            Debug.Log($"Remove intel {p_intel?.log.logText}");
        }
    }
    private void OnHoverOverBlackmail(IIntel p_blackmail, UIHoverPosition p_hoverPosition) {
        string blackmailText = p_blackmail.GetIntelInfoBlackmailText();
        string reactionText = p_blackmail.GetIntelInfoRelationshipText();
        string text = string.Empty;

        text += blackmailText;
        if (!string.IsNullOrEmpty(text)) {
            text += "\n";
        }
        text += reactionText;

        if (!string.IsNullOrEmpty(text)) {
            UIManager.Instance.ShowSmallInfo(text, p_hoverPosition);
        }
    }
    private void OnHoverOutBlackmail(IIntel p_blackmail) {
        UIManager.Instance.HideSmallInfo();
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
