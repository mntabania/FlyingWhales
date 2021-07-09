using System;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;
using Ruinarch;

public class PortalUIController : MVCUIController, PortalUIView.IListener {
    [SerializeField]
    private PortalUIModel m_portalUIModel;
    private PortalUIView m_portalUIView;
    
    public PurchaseSkillUIController purchaseSkillUIController;
    public UnlockMinionUIController unlockMinionUIController;
    public UnlockStructureUIController unlockStructureUIController;
    public UpgradePortalUIController upgradePortalUIController;

    private string m_tooltipCancelReleaseAbility;
    private string m_tooltipCancelUpgradePortal;

    private ThePortal _portal;
    
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
            
            // unlockStructureUIController.InstantiateUI();
            // unlockStructureUIController.HideUI();
        });
    }
    public void ShowUI(ThePortal p_portal) {
        _portal = p_portal;
        ShowUI();
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        m_portalUIView.SetUpgradePortalBtnInteractable(!p_portal.IsMaxLevel());
    }
    public override void ShowUI() {
        m_mvcUIView.ShowUI();
        if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            m_portalUIView.ShowUnlockAbilityTimerAndHideButton(PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked));
        } else {
            m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
        }
        if (!PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.IsFinished()) {
            m_portalUIView.ShowUpgradePortalTimerAndHideButton();
        } else {
            m_portalUIView.ShowUpgradePortalButtonAndHideTimer();
        }
    }
    public override void HideUI() {
        base.HideUI();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
        UIManager.Instance.SetSpeedTogglesState(true);
        UIManager.Instance.ResumeLastProgressionSpeed();
    }
    private void Start() {
        Messenger.AddListener(Signals.GAME_LOADED, Initialize);
    }
    private void Initialize() {
        InstantiateUI();
        HideUI();
        int orderInHierarchy = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
        m_portalUIView.UIModel.transform.SetSiblingIndex(orderInHierarchy);

        m_tooltipCancelReleaseAbility = LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "cancel_release_ability");
        m_tooltipCancelUpgradePortal = LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "cancel_upgrade_portal");
        Messenger.RemoveListener(Signals.GAME_LOADED, Initialize);
    }
    public void InitializeAfterLoadoutSelected() {
        m_portalUIView.UIModel.timerReleaseAbility.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell);
        m_portalUIView.UIModel.timerReleaseAbility.SetHoverOverAction(OnHoverOverReleaseAbilityTimer);
        m_portalUIView.UIModel.timerReleaseAbility.SetHoverOutAction(OnHoverOutReleaseAbilityTimer);
        m_portalUIView.UIModel.timerUpgradePortal.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal);
        m_portalUIView.UIModel.timerUpgradePortal.SetHoverOverAction(OnHoverOverUpgradePortalTimer);
        m_portalUIView.UIModel.timerUpgradePortal.SetHoverOutAction(OnHoverOutUpgradePortalTimer);
    }

    #region Listeners
    private void SubscribeListeners() {
        Messenger.AddListener<SkillData, int>(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, OnPlayerChoseSkillToUnlock);
        Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, OnPlayerFinishedSkillUnlock);
        Messenger.AddListener(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED, OnPlayerCancelledSkillUnlock);
        
        Messenger.AddListener(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE, OnPlayerChosePortalUpgrade);
        Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
        Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
    }
    private void OnPlayerChoseSkillToUnlock(SkillData p_skill, int p_unlockCost) {
        m_portalUIView.ShowUnlockAbilityTimerAndHideButton(p_skill);
    }
    private void OnPlayerFinishedSkillUnlock(PLAYER_SKILL_TYPE p_skill, int p_unlockCost) {
        m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
        // purchaseSkillUIController.OnFinishSkillUnlock();
    }
    private void OnPlayerCancelledSkillUnlock() {
        m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
    }
    
    private void OnPlayerChosePortalUpgrade() {
        m_portalUIView.ShowUpgradePortalTimerAndHideButton();
    }
    private void OnPlayerFinishedPortalUpgrade(int p_currentPortal) {
        m_portalUIView.ShowUpgradePortalButtonAndHideTimer();
    }
    private void OnPlayerCancelledPortalUpgrade() {
        m_portalUIView.ShowUpgradePortalButtonAndHideTimer();
    }
    #endregion
    
    public void OnClickReleaseAbility() {
        purchaseSkillUIController.Init(purchaseSkillUIController.skillCountPerDraw, true);
    }
    public void OnClickUpgradePortal() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        upgradePortalUIController.ShowPortalUpgradeTier(portal.nextTier, portal.level, portal);
    }
    public void OnClickCancelReleaseAbility() {
        SkillData spellData = PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked);
        UIManager.Instance.ShowYesNoConfirmation(
            "Cancel Release Ability", $"Are you sure you want to cancel Releasing Ability: <b>{spellData.localizedName}</b>? " + 
                                      $"\n<i>{UtilityScripts.Utilities.InvalidColorize("Cancelling will reset all current release progress!")}</i>", OnConfirmCancelRelease, showCover: true, layer: 30);
        // purchaseSkillUIController.Init(purchaseSkillUIController.skillCountPerDraw);
    }
    private void OnConfirmCancelRelease() {
        PlayerManager.Instance.player.playerSkillComponent.CancelCurrentPlayerSkillUnlock();
    }
    public void OnClickCancelUpgradePortal() {
        UIManager.Instance.ShowYesNoConfirmation(
            "Cancel Portal Upgrade", $"Are you sure you want to cancel Portal Upgrade? " + 
                                      $"\n<i>{UtilityScripts.Utilities.InvalidColorize("Cancelling will reset all current upgrade progress!")}</i>", OnConfirmCancelUpgradePortal, showCover: true, layer: 30);
    }
    private void OnConfirmCancelUpgradePortal() {
        PlayerManager.Instance.player.playerSkillComponent.CancelPortalUpgrade();
    }
    public void OnHoverOverCancelReleaseAbility() {
        UIManager.Instance.ShowSmallInfo(m_tooltipCancelReleaseAbility);
    }
    public void OnHoverOutCancelReleaseAbility() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverOverCancelUpgradePortal() {
        UIManager.Instance.ShowSmallInfo(m_tooltipCancelUpgradePortal);
    }
    public void OnHoverOutCancelUpgradePortal() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverOverUpgradePortal() {
        if (_portal != null && _portal.IsMaxLevel()) {
            UIManager.Instance.ShowSmallInfo("Portal is already at Max Level!");
        }
    }
    public void OnHoverOutUpgradePortal() {
        if (_portal != null && _portal.IsMaxLevel()) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    public void OnClickClose() {
        HideUI();
    }
    private void OnHoverOverReleaseAbilityTimer() {
        string message = $"Remaining time: {PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.GetRemainingTimeString()}";
        // if (PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.IsFinished()) {
        //     message = $"{message}\nReroll Available!";  
        // } else {
        //     message = $"{message}\nRemaining time until reroll: {PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.GetRemainingTimeString()}";
        // }
        UIManager.Instance.ShowSmallInfo(message, autoReplaceText: false);
    }
    private void OnHoverOutReleaseAbilityTimer() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnHoverOverUpgradePortalTimer() {
        string message = $"Remaining time: {PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.GetRemainingTimeString()}";
        UIManager.Instance.ShowSmallInfo(message, autoReplaceText: false);
    }
    private void OnHoverOutUpgradePortalTimer() {
        UIManager.Instance.HideSmallInfo();
    }
}
