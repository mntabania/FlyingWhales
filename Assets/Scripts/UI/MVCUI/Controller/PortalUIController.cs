using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PortalUIController : MVCUIController, PortalUIView.IListener {
    [SerializeField]
    private PortalUIModel m_portalUIModel;
    private PortalUIView m_portalUIView;
    
    public PurchaseSkillUIController purchaseSkillUIController;
    public UnlockMinionUIController unlockMinionUIController;
    public UnlockStructureUIController unlockStructureUIController;
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        PortalUIView.Create(_canvas, m_portalUIModel, (p_ui) => {
            m_portalUIView = p_ui;
            m_portalUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            SubscribeListeners();

            unlockMinionUIController.InstantiateUI();
            unlockMinionUIController.HideUI();
            
            unlockStructureUIController.InstantiateUI();
            unlockStructureUIController.HideUI();
        });
    }
    public override void ShowUI() {
        m_mvcUIView.ShowUI();
        if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            m_portalUIView.ShowUnlockAbilityTimerAndHideButton(PlayerSkillManager.Instance.GetPlayerSkillData(PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked));
        } else {
            m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
        }
        if (PlayerManager.Instance.player.playerSkillComponent.currentDemonBeingSummoned != PLAYER_SKILL_TYPE.NONE) {
            m_portalUIView.ShowUnlockDemonTimerAndHideButton(PlayerSkillManager.Instance.GetPlayerSkillData(PlayerManager.Instance.player.playerSkillComponent.currentDemonBeingSummoned));
        } else {
            m_portalUIView.ShowUnlockDemonButtonAndHideTimer();
        }
        if (PlayerManager.Instance.player.playerSkillComponent.currentStructureBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            m_portalUIView.ShowUnlockStructureTimerAndHideButton(PlayerSkillManager.Instance.GetPlayerSkillData(PlayerManager.Instance.player.playerSkillComponent.currentStructureBeingUnlocked));
        } else {
            m_portalUIView.ShowUnlockStructureButtonAndHideTimer();
        }
    }
    private void Start() {
        UIManager.Instance.onPortalClicked += OnPortalClicked;
        UIManager.Instance.structureInfoUI.AddCloseMenuAction(HideUI);
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
        InstantiateUI();
        HideUI();
        int orderInHierarchy = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
        m_portalUIView.UIModel.transform.SetSiblingIndex(orderInHierarchy);
        
        Messenger.RemoveListener(Signals.GAME_LOADED, Initialize);
    }
    public void InitializeAfterLoadoutSelected() {
        m_portalUIView.UIModel.timerReleaseAbility.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell);
        m_portalUIView.UIModel.timerSummonDemon.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerSummonDemon);
        m_portalUIView.UIModel.timerObtainBlueprint.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockStructure);
    }

    #region Listeners
    private void SubscribeListeners() {
        Messenger.AddListener<SkillData, int>(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, OnPlayerChoseSkillToUnlock);
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, OnPlayerFinishedSkillUnlock);
        Messenger.AddListener(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED, OnPlayerCancelledSkillUnlock);
        
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_CHOSE_DEMON_TO_UNLOCK, OnPlayerChoseDemonToUnlock);
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_DEMON_UNLOCK, OnPlayerFinishedDemonUnlock);
        Messenger.AddListener(PlayerSignals.PLAYER_DEMON_UNLOCK_CANCELLED, OnPlayerCancelledDemonUnlock);
        
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_CHOSE_STRUCTURE_TO_UNLOCK, OnPlayerChoseStructureToUnlock);
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_STRUCTURE_UNLOCK, OnPlayerFinishedStructureUnlock);
        Messenger.AddListener(PlayerSignals.PLAYER_STRUCTURE_UNLOCK_CANCELLED, OnPlayerCancelledStructureUnlock);
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
    
    private void OnPlayerChoseDemonToUnlock(PLAYER_SKILL_TYPE p_minionType, int p_unlockCost) {
        m_portalUIView.ShowUnlockDemonTimerAndHideButton(PlayerSkillManager.Instance.GetPlayerSkillData(p_minionType));
    }
    private void OnPlayerFinishedDemonUnlock(PLAYER_SKILL_TYPE p_minionType, int p_unlockCost) {
        m_portalUIView.ShowUnlockDemonButtonAndHideTimer();
    }
    private void OnPlayerCancelledDemonUnlock() {
        m_portalUIView.ShowUnlockDemonButtonAndHideTimer();
    }
    
    private void OnPlayerChoseStructureToUnlock(PLAYER_SKILL_TYPE p_structureType, int p_unlockCost) {
        m_portalUIView.ShowUnlockStructureTimerAndHideButton(PlayerSkillManager.Instance.GetPlayerSkillData(p_structureType));
    }
    private void OnPlayerFinishedStructureUnlock(PLAYER_SKILL_TYPE p_structureType, int p_unlockCost) {
        m_portalUIView.ShowUnlockStructureButtonAndHideTimer();
    }
    private void OnPlayerCancelledStructureUnlock() {
        m_portalUIView.ShowUnlockStructureButtonAndHideTimer();
    }
    #endregion
    
    public void OnClickReleaseAbility() {
        purchaseSkillUIController.Init(purchaseSkillUIController.skillCountPerDraw);
    }
    public void OnClickSummonDemon() {
        unlockMinionUIController.ShowUI();
    }
    public void OnClickObtainBlueprint() {
        unlockStructureUIController.ShowUI();
    }
    public void OnClickCancelReleaseAbility() {
        PlayerManager.Instance.player.playerSkillComponent.CancelCurrentPlayerSkillUnlock();
        purchaseSkillUIController.Init(purchaseSkillUIController.skillCountPerDraw);
    }
    public void OnClickCancelSummonDemon() {
        PlayerManager.Instance.player.playerSkillComponent.CancelCurrentMinionUnlock();
        unlockMinionUIController.ShowUI();
    }
    public void OnClickCancelObtainBlueprint() {
        PlayerManager.Instance.player.playerSkillComponent.CancelCurrentStructureUnlock();
        unlockStructureUIController.ShowUI();
    }
}
