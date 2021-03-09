using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PortalUIController : MVCUIController, PortalUIView.IListener {
    [SerializeField]
    private PortalUIModel m_portalUIModel;
    private PortalUIView m_portalUIView;
    
    public PurchaseSkillUIController purchaseSkillUIController;
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        PortalUIView.Create(_canvas, m_portalUIModel, (p_ui) => {
            m_portalUIView = p_ui;
            m_portalUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            SubscribeListeners();
        });
    }
    private void Start() {
        UIManager.Instance.onPortalClicked += OnPortalClicked;
        UIManager.Instance.structureInfoUI.AddCloseMenuAction(HideUI);
        InstantiateUI();
        HideUI();
        Messenger.AddListener(Signals.GAME_LOADED, Initialize);
    }
    private void OnDestroy() {
        if (UIManager.Instance != null) {
            UIManager.Instance.onPortalClicked -= OnPortalClicked;    
        }
    }
    private void OnPortalClicked() {
        if (GameManager.Instance.gameHasStarted) {
            ShowUI();
        }
    }
    private void Initialize() {
        Messenger.RemoveListener(Signals.GAME_LOADED, Initialize);
        m_portalUIView.UIModel.timerReleaseAbility.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell);
        m_portalUIView.UIModel.timerSummonDemon.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerSummonDemon);
        m_portalUIView.UIModel.timerObtainBlueprint.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerObtainBlueprint);
    }

    #region Listeners
    private void SubscribeListeners() {
        Messenger.AddListener<SkillData, int>(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, OnPlayerChoseSkillToUnlock);
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, OnPlayerFinishedSkillUnlock);
        Messenger.AddListener(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED, OnPlayerCancelledSkillUnlock);
    }
    private void OnPlayerChoseSkillToUnlock(SkillData p_skill, int p_unlockCost) {
        m_portalUIView.ShowUnlockAbilityTimerAndHideButton(p_skill);
    }
    private void OnPlayerFinishedSkillUnlock(PLAYER_SKILL_TYPE p_skill, int p_unlockCost) {
        m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
        purchaseSkillUIController.OnFinishSkillUnlock();
    }
    private void OnPlayerCancelledSkillUnlock() {
        m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
    }
    #endregion
    
    public void OnClickReleaseAbility() {
        purchaseSkillUIController.Init(purchaseSkillUIController.skillCountPerDraw);
    }
    public void OnClickSummonDemon() {
        throw new System.NotImplementedException();
    }
    public void OnClickObtainBlueprint() {
        throw new System.NotImplementedException();
    }
    public void OnClickCancelReleaseAbility() {
        PlayerManager.Instance.player.playerSkillComponent.CancelCurrentPlayerSkillUnlock();
    }
    public void OnClickCancelSummonDemon() {
        throw new System.NotImplementedException();
    }
    public void OnClickCancelObtainBlueprint() {
        throw new System.NotImplementedException();
    }
}
