using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TemptUIController : MVCUIController, TemptUIView.IListener {
    [SerializeField] private TemptUIModel m_temptUIModel;
    private TemptUIView m_temptUIView;

    private List<TEMPTATION> _chosenTemptations;
    private System.Action<List<TEMPTATION>> _onConfirmAction;

    private Character _targetCharacter;

    private void Awake() {
        _chosenTemptations = new List<TEMPTATION>();
    }

    #region Overrides
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        TemptUIView.Create(_canvas, m_temptUIModel, (p_ui) => {
            m_temptUIView = p_ui;
            m_temptUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
        });
    }
    public override void ShowUI() {
        base.ShowUI();
        _chosenTemptations.Clear();
    }
    #endregion

    public bool HasValidTemptationsForTarget(Character p_target) {
        for (int i = 0; i < m_temptUIView.allTemptationTypes.Length; i++) {
            TEMPTATION temptation = m_temptUIView.allTemptationTypes[i];
            if (temptation.CanTemptCharacter(p_target)) {
                return true;
            }
        }
        return false;
    }
    
    public void ShowTemptationPopup(Character p_target, Action<List<TEMPTATION>> p_onConfirmAction, List<TEMPTATION> p_alreadyChosenTemptations) {
        _targetCharacter = p_target;
        ShowUI();
        m_temptUIView.UpdateShownItems(p_target, p_alreadyChosenTemptations);
        _onConfirmAction = p_onConfirmAction;
        Messenger.Broadcast(UISignals.TEMPTATIONS_POPUP_SHOWN);
    }

    #region TemptUIView.IListener Implementation
    public void OnToggleDarkBlessing(bool p_isOn) {
        if (p_isOn) {
            _chosenTemptations.Add(TEMPTATION.Dark_Blessing);
            Debug.Log("Added Dark Blessing");
        } else {
            _chosenTemptations.Remove(TEMPTATION.Dark_Blessing);
            Debug.Log("Removed Dark Blessing");
        }
    }
    public void OnToggleEmpower(bool p_isOn) {
        if (p_isOn) {
            _chosenTemptations.Add(TEMPTATION.Empower);
            Debug.Log("Added Empower");
        } else {
            _chosenTemptations.Remove(TEMPTATION.Empower);
            Debug.Log("Removed Empower");
        }
    }
    public void OnToggleCleanseFlaws(bool p_isOn) {
        if (p_isOn) {
            _chosenTemptations.Add(TEMPTATION.Cleanse_Flaws);
            Debug.Log("Added Cleanse Flaws");
        } else {
            _chosenTemptations.Remove(TEMPTATION.Cleanse_Flaws);
            Debug.Log("Removed Cleanse Flaws");
        }
    }
    public void OnHoverDarkBlessing() {
        string text = $"Offer to grant Dark Blessing to {_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} - a promise to protect them from further interferences.";
        UIManager.Instance.ShowSmallInfo(text, autoReplaceText: false);
    }
    public void OnHoverEmpower() {
        string text = $"Offer to make {_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} mighty - increasing HP and Strength.";
        UIManager.Instance.ShowSmallInfo(text, autoReplaceText: false);
    }
    public void OnHoverCleanseFlaws() {
        string text = $"Offer to remove all Flaws of {_targetCharacter.visuals.GetCharacterNameWithIconAndColor()}.";
        UIManager.Instance.ShowSmallInfo(text, autoReplaceText: false);
    }
    public void OnHoverOutTemptation() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnClickClose() {
        HideUI();
    }
    public void OnClickConfirm() {
        HideUI();
        _onConfirmAction?.Invoke(_chosenTemptations);
    }
    #endregion
}
