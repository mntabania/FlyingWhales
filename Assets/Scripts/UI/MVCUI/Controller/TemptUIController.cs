using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TemptUIController : MVCUIController, TemptUIView.IListener {
    [SerializeField] private TemptUIModel m_temptUIModel;
    private TemptUIView m_temptUIView;

    private List<TEMPTATION> _chosenTemptations;
    private void Start() {
        InstantiateUI();
    }
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

    public void ShowTemptationPopup(Character p_target) {
        ShowUI();
        m_temptUIView.UpdateShownItems(p_target);
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
    public void OnClickClose() {
        HideUI();
    }
    public void OnClickConfirm() {
        HideUI();
    }
    #endregion
}
