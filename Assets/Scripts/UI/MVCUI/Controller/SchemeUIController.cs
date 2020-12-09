using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SchemeUIController : MVCUIController, SchemeUIView.IListener {
    private enum BLACKMAIL_TYPE {
        None, Strong, Normal, Weak,
    }

    [SerializeField] private SchemeUIModel m_schemeUIModel;
    private SchemeUIView m_schemeUIView;

    private Character _targetCharacter;
    private object _otherTarget;
    private SchemeData _schemeUsed;
    private float _successRate;
    private List<SchemeUIItem> _schemeUIItems;

    private System.Action<IIntel> _onConfirmAction;
    
    private void Start() {
        InstantiateUI();
        HideUI();
    }

    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        SchemeUIView.Create(_canvas, m_schemeUIModel, (p_ui) => {
            m_schemeUIView = p_ui;
            m_schemeUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
        });
    }

    public void Show(Character p_targetCharacter, object p_otherTarget, SchemeData p_schemeUsed) {
        ShowUI();
        _targetCharacter = p_targetCharacter;
        _otherTarget = p_otherTarget;
        _schemeUsed = p_schemeUsed;
        if(_schemeUIItems == null) {
            _schemeUIItems = new List<SchemeUIItem>();
        }
        ClearSchemeUIItems();
        m_schemeUIView.SetTitle(_schemeUsed.name);
    }

    #region Blackmail
    public void AddBlackmail(IIntel intel) {
        BLACKMAIL_TYPE blackmailType = GetBlackMailTypeConsidering(intel);
        if(blackmailType == BLACKMAIL_TYPE.None) {
            CreateAndAddNewSchemeUIItem("Non-Blackmail Material", 0f, OnClickMinusSchemeUIItem, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
            UpdateSuccessRate();
        } else {
            float successRate = GetSchemeSuccessRate(blackmailType);
            CreateAndAddNewSchemeUIItem($"{blackmailType.ToString()} Blackmail", successRate, OnClickMinusSchemeUIItem, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
            UpdateSuccessRate();
        }
    }
    private BLACKMAIL_TYPE GetBlackMailTypeConsidering(IIntel intel) {
        CRIME_TYPE crimeType = intel.reactable.crimeType;
        if(crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset && _targetCharacter.faction != null) {
            CRIME_SEVERITY severity = _targetCharacter.faction.GetCrimeSeverity(intel.actor, intel.target, crimeType);
            if(severity == CRIME_SEVERITY.Heinous) {
                return BLACKMAIL_TYPE.Strong;
            } else if (severity == CRIME_SEVERITY.Serious) {
                return BLACKMAIL_TYPE.Normal;
            } else if (severity == CRIME_SEVERITY.Misdemeanor || severity == CRIME_SEVERITY.Infraction) {
                return BLACKMAIL_TYPE.Weak;
            }
        }
        return BLACKMAIL_TYPE.None;
    }
    #endregion

    #region Temptation
    //TODO: Add Temptation
    #endregion

    #region Scheme
    //TODO: GetSchemeSuccessRate Temptation
    private float GetSchemeSuccessRate(BLACKMAIL_TYPE blackmailType) {
        float rate = 0f;
        if(blackmailType == BLACKMAIL_TYPE.Strong) {
            rate = 50f;
        } else if (blackmailType == BLACKMAIL_TYPE.Normal) {
            rate = 35f;
        } else if (blackmailType == BLACKMAIL_TYPE.Weak) {
            rate = 20f;
        }
        rate *= GetSchemeSuccessRateMultiplier();
        return rate;
    }
    private float GetSchemeSuccessRateMultiplier() {
        return _schemeUsed.GetSuccessRateMultiplier(_targetCharacter);
    }
    #endregion

    #region SchemeUIView.IListener Implementation
    public void OnClickClose() {
        HideUI();
    }
    public void OnClickConfirm() {
        HideUI();
        //Process all activate temptations also
        //Consume blackmail intels
        _schemeUsed.ProcessScheme(_targetCharacter, _otherTarget, _successRate);
    }
    public void OnClickBlackmail() {
        //Show Blackmail UI
    }
    public void OnClickTemptation() {
        //Show Temptation UI
    }
    #endregion

    #region SchemeUIItem
    private void ClearSchemeUIItems() {
        for (int i = 0; i < _schemeUIItems.Count; i++) {
            SchemeUIItem item = _schemeUIItems[i];
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }
        _schemeUIItems.Clear();
        UpdateSuccessRate();
    }
    private void RemoveSchemeUIItem(SchemeUIItem item) {
        if (_schemeUIItems.Remove(item)) {
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
            UpdateSuccessRate();
        }
    }
    private void CreateAndAddNewSchemeUIItem(string text, float successRate, System.Action<SchemeUIItem> onClickMinusAction, System.Action<SchemeUIItem> onHoverEnterAction, System.Action<SchemeUIItem> onHoverExitAction) {
        GameObject go = UIManager.Instance.InstantiateUIObject(m_schemeUIModel.schemeUIItemPrefab.name, m_schemeUIModel.scrollViewSchemes.content);
        SchemeUIItem item = go.GetComponent<SchemeUIItem>();
        item.SetItemDetails(text, successRate);
        item.SetClickMinusAction(onClickMinusAction);
        item.SetOnHoverEnterAction(onHoverEnterAction);
        item.SetOnHoverExitAction(onHoverExitAction);
        _schemeUIItems.Add(item);
    }
    private void OnClickMinusSchemeUIItem(SchemeUIItem item) {
        RemoveSchemeUIItem(item);
    }
    private void OnHoverEnterSchemeUIItem(SchemeUIItem item) {
        string text = _schemeUsed.GetSuccessRateMultiplierText(_targetCharacter);
        if (!string.IsNullOrEmpty(text)) {
            UIManager.Instance.ShowSmallInfo(text);
        }
    }
    private void OnHoverExitSchemeUIItem(SchemeUIItem item) {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Success Rate
    private void UpdateSuccessRate() {
        float successRate = 0f;
        for (int i = 0; i < _schemeUIItems.Count; i++) {
            successRate += _schemeUIItems[i].successRate;
        }
        _successRate = successRate;

        float successRateForText = successRate;
        successRateForText = Mathf.Clamp(successRateForText, 0f, 100f);
        m_schemeUIView.SetSuccessRate($"{successRateForText.ToString("N2")}");
    }
    #endregion
}
