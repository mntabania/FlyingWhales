using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;
using Traits;

public class SchemeUIController : MVCUIController, SchemeUIView.IListener {

    [SerializeField] private SchemeUIModel m_schemeUIModel;
    private SchemeUIView m_schemeUIView;

    public BlackmailUIController blackmailUIController;
    public TemptUIController temptUIController;

    private Character _targetCharacter;
    private object _otherTarget;
    private SchemeData _schemeUsed;
    private float _successRate;
    private List<SchemeUIItem> _schemeUIItems;
    private List<IIntel> _chosenBlackmail;
    private List<TEMPTATION> _chosenTemptations;
    
    private System.Action _onCloseAction;
    private void Start() {
        InstantiateUI();
        HideUI();
    }
    private void Awake() {
        _chosenBlackmail = new List<IIntel>();
        _chosenTemptations = new List<TEMPTATION>();
    }

    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        SchemeUIView.Create(_canvas, m_schemeUIModel, (p_ui) => {
            m_schemeUIView = p_ui;
            m_schemeUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            
            blackmailUIController.InstantiateUI();
            blackmailUIController.HideUI();
            temptUIController.InstantiateUI();
            temptUIController.HideUI();
        });
    }

    public void Show(Character p_targetCharacter, object p_otherTarget, SchemeData p_schemeUsed, System.Action p_onCloseAction) {
        ShowUI();
        _targetCharacter = p_targetCharacter;
        _otherTarget = p_otherTarget;
        _schemeUsed = p_schemeUsed;
        if(_schemeUIItems == null) {
            _schemeUIItems = new List<SchemeUIItem>();
        }
        _onCloseAction = p_onCloseAction;
        _chosenBlackmail.Clear();
        _chosenTemptations.Clear();
        ClearSchemeUIItems();
        m_schemeUIView.SetTitle(_schemeUsed.localizedName);
        m_schemeUIView.SetBlackmailBtnInteractableState(HasValidBlackmailForTarget(p_targetCharacter));
        m_schemeUIView.SetTemptBtnInteractableState(temptUIController.HasValidTemptationsForTarget(p_targetCharacter));

        if (_targetCharacter.traitContainer.HasTrait("Cultist")) {
            //Cultist have a base success rate of 100% in all schemes
            float baseSuccessRate = 100f;
            float successRate = baseSuccessRate;
            ProcessSchemeSuccessRateWithMultipliers(ref successRate);
            SchemeUIItem item = CreateAndAddNewSchemeUIItem("Cultist", successRate, baseSuccessRate, null, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
            item.btnMinus.interactable = false;
            UpdateSuccessRate();
        }
    }
    public override void HideUI() {
        base.HideUI();
        _onCloseAction?.Invoke();
    }

    #region Blackmail
    private void AddBlackmail(IIntel p_intel) {
        BLACKMAIL_TYPE blackmailType = p_intel.GetBlackMailTypeConsideringTarget(_targetCharacter);
        if(blackmailType == BLACKMAIL_TYPE.None) {
            CreateAndAddNewSchemeUIItem("Non-Blackmail Material", 0f, 0f, item => {
                OnClickMinusSchemeUIItem(item);
                _chosenBlackmail.Remove(p_intel);
            }, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
        } else {
            float successRate = GetSchemeSuccessRate(blackmailType);
            float baseSuccessRate = GetBaseSchemeSuccessRate(blackmailType);
            CreateAndAddNewSchemeUIItem($"{blackmailType.ToString()} Blackmail", successRate, baseSuccessRate, item => {
                OnClickMinusSchemeUIItem(item);
                _chosenBlackmail.Remove(p_intel);
            }, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
        }
        _chosenBlackmail.Add(p_intel);
        UpdateSuccessRate();
    }
    private List<IIntel> GetValidBlackmailForTarget(Character p_target) {
        List<IIntel> validIntel = null;
        for (int i = 0; i < PlayerManager.Instance.player.allIntel.Count; i++) {
            IIntel intel = PlayerManager.Instance.player.allIntel[i];
            if (intel.CanBeUsedToBlackmailCharacter(p_target)) {
                if (validIntel == null) { validIntel = new List<IIntel>(); }
                validIntel.Add(intel);
            }
        }
        return validIntel;
    }
    private bool HasValidBlackmailForTarget(Character p_target) {
        for (int i = 0; i < PlayerManager.Instance.player.allIntel.Count; i++) {
            IIntel intel = PlayerManager.Instance.player.allIntel[i];
            if (intel.CanBeUsedToBlackmailCharacter(p_target)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Temptation
    private void AddTemptation(TEMPTATION p_temptation) {
        float successRate = GetSchemeSuccessRate(p_temptation);
        float baseSuccessRate = GetBaseSchemeSuccessRate(p_temptation);
        CreateAndAddNewSchemeUIItem(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_temptation.ToString()), successRate, baseSuccessRate, item => {
            OnClickMinusSchemeUIItem(item);
            _chosenTemptations.Remove(p_temptation);
        }, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
        
        _chosenTemptations.Add(p_temptation);
        UpdateSuccessRate();
    }
    private void ActivateTemptationEffect(TEMPTATION p_temptation) {
        switch (p_temptation) {
            case TEMPTATION.Dark_Blessing:
                _targetCharacter.traitContainer.AddTrait(_targetCharacter, "Dark Blessing");
                break;
            case TEMPTATION.Empower:
                _targetCharacter.traitContainer.AddTrait(_targetCharacter, "Mighty");
                break;
            case TEMPTATION.Cleanse_Flaws:
                _targetCharacter.traitContainer.RemoveAllTraitsByType(_targetCharacter, TRAIT_TYPE.FLAW);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(p_temptation), p_temptation, null);
        }
    }
    #endregion

    #region Scheme
    private float GetSchemeSuccessRate(TEMPTATION temptationType) {
        float rate = GetBaseSchemeSuccessRate(temptationType);
        ProcessSchemeSuccessRateWithMultipliers(ref rate);
        return rate;
    }
    private float GetBaseSchemeSuccessRate(TEMPTATION temptationType) {
        float rate = 0f;
        if (temptationType == TEMPTATION.Dark_Blessing) {
            rate = 50f;
        } else if (temptationType == TEMPTATION.Empower) {
            rate = 25f;
        } else if (temptationType == TEMPTATION.Cleanse_Flaws) {
            int flawCount = 0;
            for (int i = 0; i < _targetCharacter.traitContainer.traits.Count; i++) {
                Trait trait = _targetCharacter.traitContainer.traits[i];
                if (trait.type == TRAIT_TYPE.FLAW) {
                    flawCount++;
                }
            }
            rate = 20f * flawCount;
        }
        return rate;
    }
    private float GetSchemeSuccessRate(BLACKMAIL_TYPE blackmailType) {
        float rate = GetBaseSchemeSuccessRate(blackmailType);
        ProcessSchemeSuccessRateWithMultipliers(ref rate);
        return rate;
    }
    private float GetBaseSchemeSuccessRate(BLACKMAIL_TYPE blackmailType) {
        float rate = 0f;
        if (blackmailType == BLACKMAIL_TYPE.Strong) {
            rate = 50f;
        } else if (blackmailType == BLACKMAIL_TYPE.Normal) {
            rate = 35f;
        } else if (blackmailType == BLACKMAIL_TYPE.Weak) {
            rate = 20f;
        }
        return rate;
    }
    private void ProcessSchemeSuccessRateWithMultipliers(ref float rate) {
        _schemeUsed.ProcessSuccessRateWithMultipliers(_targetCharacter, ref rate);
    }
    #endregion

    #region SchemeUIView.IListener Implementation
    public void OnClickClose() {
        HideUI();
    }
    public void OnClickConfirm() {
        HideUI();
        //Consume blackmail intels
        for (int i = 0; i < _chosenBlackmail.Count; i++) {
            IIntel blackmail = _chosenBlackmail[i];
            PlayerManager.Instance.player.RemoveIntel(blackmail);
        }
        bool isSuccessful = _schemeUsed.ProcessScheme(_targetCharacter, _otherTarget, _successRate);
        
        //Only activate temptation effects if scheme is successful
        if (isSuccessful) {
            //Process all activate temptations also
            Messenger.Broadcast(CharacterSignals.CHARACTER_MEDDLER_SCHEME_SUCCESSFUL, _targetCharacter);
            if (_chosenTemptations.Count > 0) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Schemes", "General", "tempted", null, LOG_TAG.Player);
                string temptations = string.Empty;
                for (int i = 0; i < _chosenTemptations.Count; i++) {
                    TEMPTATION temptation = _chosenTemptations[i];
                    if (i == _chosenTemptations.Count - 1) {
                        temptations += ", and ";
                    } else if (i > 0) {
                        temptations += ", ";
                    }
                    temptations += UtilityScripts.Utilities.NotNormalizedConversionEnumToString(temptation.ToString());
                    ActivateTemptationEffect(temptation);
                }
                log.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, temptations, LOG_IDENTIFIER.STRING_1);
                log.AddLogToDatabase();
                //_targetCharacter.logComponent.RegisterLog(log);
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }
        }
    }
    public void OnClickBlackmail() {
        //Show Blackmail UI
        List<IIntel> validIntel = GetValidBlackmailForTarget(_targetCharacter);
        if (validIntel != null) {
            blackmailUIController.ShowBlackmailUI(validIntel, _chosenBlackmail, OnConfirmBlackmail);    
        }
    }
    private void OnConfirmBlackmail(List<IIntel> p_chosenBlackmail) {
        for (int i = 0; i < p_chosenBlackmail.Count; i++) {
            IIntel intel = p_chosenBlackmail[i];
            if (!_chosenBlackmail.Contains(intel)) {
                AddBlackmail(intel);
            }
        }
    }
    public void OnClickTemptation() {
        //Show Temptation UI
        if (temptUIController.HasValidTemptationsForTarget(_targetCharacter)) {
            temptUIController.ShowTemptationPopup(_targetCharacter, OnConfirmTemptation, _chosenTemptations);
        }
    }
    private void OnConfirmTemptation(List<TEMPTATION> p_temptations) {
        for (int i = 0; i < p_temptations.Count; i++) {
            TEMPTATION temptation = p_temptations[i];
            if (!_chosenTemptations.Contains(temptation)) {
                AddTemptation(temptation);    
            }
        }
        if (p_temptations.Count > 0) {
            Messenger.Broadcast(UISignals.TEMPTATIONS_OFFERED);
        }
    }
    public void OnHoverOverBlackmailBtn(UIHoverPosition p_hoverPos) {
        if (!HasValidBlackmailForTarget(_targetCharacter)) {
            UIManager.Instance.ShowSmallInfo(UtilityScripts.Utilities.ColorizeInvalidText($"You have no usable blackmail against {_targetCharacter.name}!"), p_hoverPos, "No Blackmail");    
        }
    }
    public void OnHoverOverTemptBtn(UIHoverPosition p_hoverPos) {
        if (!temptUIController.HasValidTemptationsForTarget(_targetCharacter)) {
            UIManager.Instance.ShowSmallInfo(UtilityScripts.Utilities.ColorizeInvalidText($"You have no usable temptations for {_targetCharacter.name}!"), p_hoverPos, "No Temptations");
        }
    }
    public void OnHoverOutBlackmailBtn() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverOutTemptBtn() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverOverSuccessRate() {
        if (_schemeUIItems.Count > 0) {
            PlayerSkillData skillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(_schemeUsed.type);
            float resistanceValue = _targetCharacter.piercingAndResistancesComponent.GetResistanceValue(skillData.resistanceType);

            if (resistanceValue > 0f) {
                string text = string.Empty;
                float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(_schemeUsed.type);
                float baseSuccessRate = 0f;
                for (int i = 0; i < _schemeUIItems.Count; i++) {
                    baseSuccessRate += _schemeUIItems[i].successRate;
                }
                text += $"Base Success Rate: {baseSuccessRate.ToString("N1")}%";
                text += $"\nPiercing: {piercing.ToString("N2")}%";
                text += $"\nResistance: {resistanceValue.ToString("N2")}%";
                UIManager.Instance.ShowSmallInfo(text);
            }
        }
    }
    public void OnHoverOutSuccessRate() {
        UIManager.Instance.HideSmallInfo();
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
    private SchemeUIItem CreateAndAddNewSchemeUIItem(string p_text, float p_successRate, float p_baseSuccessRate, System.Action<SchemeUIItem> p_onClickMinusAction, System.Action<SchemeUIItem> p_onHoverEnterAction, System.Action<SchemeUIItem> p_onHoverExitAction) {
        GameObject go = UIManager.Instance.InstantiateUIObject(m_schemeUIView.UIModel.schemeUIItemPrefab.name, m_schemeUIView.UIModel.scrollViewSchemes.content);
        SchemeUIItem item = go.GetComponent<SchemeUIItem>();
        item.SetItemDetails(p_text, p_successRate, p_baseSuccessRate);
        item.SetClickMinusAction(p_onClickMinusAction);
        item.SetOnHoverEnterAction(p_onHoverEnterAction);
        item.SetOnHoverExitAction(p_onHoverExitAction);
        _schemeUIItems.Add(item);
        return item;
    }
    private void OnClickMinusSchemeUIItem(SchemeUIItem item) {
        RemoveSchemeUIItem(item);
    }
    private void OnHoverEnterSchemeUIItem(SchemeUIItem item) {
        string successRate = $"<b><size=18>Success Rate: <color=green>+{item.successRate.ToString("N1")}%</color></size></b>";
        string baseText = $"Base Value: <color=white>+{item.baseSuccessRate.ToString("N1")}%</color>";
        string multiplierText = _schemeUsed.GetSuccessRateMultiplierText(_targetCharacter);
        string text = "<line-height=100%>" + successRate + "\n<line-height=70%>" + baseText + "\n<line-height=70%>" + multiplierText;
        UIManager.Instance.ShowSmallInfo(text);
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
        PlayerSkillData skillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(_schemeUsed.type);
        float resistanceValue = _targetCharacter.piercingAndResistancesComponent.GetResistanceValue(skillData.resistanceType);
        CombatManager.ModifyValueByPiercingAndResistance(ref successRate, PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(_schemeUsed.type), resistanceValue);

        _successRate = successRate;

        float successRateForText = successRate;
        successRateForText = Mathf.Clamp(successRateForText, 0f, 100f);
        m_schemeUIView.SetSuccessRate($"<color=green>+{successRateForText.ToString("N1")}%</color>");
    }
    #endregion
}
