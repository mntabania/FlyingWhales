using System;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UpgradePortalUIController : MVCUIController, UpgradePortalUIView.IListener {
    [SerializeField]
    private UpgradePortalUIModel m_upgradePortalUIModel;
    private UpgradePortalUIView m_upgradePortalUIView;
    
    private string m_tooltipCancelUpgradePortal;
    public bool isShowing { get; private set; }
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        UpgradePortalUIView.Create(_canvas, m_upgradePortalUIModel, (p_ui) => {
            m_upgradePortalUIView = p_ui;
            m_upgradePortalUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
        });
    }
    private void Start() {
        InstantiateUI();
        HideUI();
        m_upgradePortalUIView.AddHoverOverActionToItems(OnHoverOverUpgradeItem);
        m_upgradePortalUIView.AddHoverOutActionToItems(OnHoverOutUpgradeItem);
        SubscribeListeners();
    }
    private void SubscribeListeners() {
        Messenger.AddListener(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE, OnPlayerChosePortalUpgrade);
        Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyAdjusted);
    }
    private void OnSpiritEnergyAdjusted(int p_amount, int p_spiritEnergy) {
        m_upgradePortalUIView.SetCurrentSpiritEnergyText(PlayerManager.Instance.player.spiritEnergy);
    }
    public void InitializeAfterLoadoutSelected() {
        m_tooltipCancelUpgradePortal = LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "cancel_upgrade_portal");
        m_upgradePortalUIView.UIModel.timerUpgradePortal.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal);
        m_upgradePortalUIView.UIModel.timerUpgradePortal.SetHoverOverAction(OnHoverOverUpgradePortalTimer);
        m_upgradePortalUIView.UIModel.timerUpgradePortal.SetHoverOutAction(OnHoverOutUpgradePortalTimer);
    }
    public void ShowPortalUpgradeTier(PortalUpgradeTier p_upgradeTier, int p_level, ThePortal portal) {
        ShowUI();
        m_upgradePortalUIView.UpdateItems(p_upgradeTier, portal);
        m_upgradePortalUIView.UpdateUpgradeChaoticEnergyItem(portal);
        m_upgradePortalUIView.UpdateAwakenRuianrchText(portal);
        m_upgradePortalUIView.SetHeader($"Upgrade to Level {(p_level + 1).ToString()}?");
        m_upgradePortalUIView.SetUpgradeText(p_upgradeTier.GetUpgradeCostString());
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
            if (PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.IsFinished()) {
                m_upgradePortalUIView.SetUpgradeBtnState(true);
                m_upgradePortalUIView.SetUpgradeTimerState(false);
            } else {
                m_upgradePortalUIView.SetUpgradeBtnState(false);
                m_upgradePortalUIView.SetUpgradeTimerState(true);
            }
            m_upgradePortalUIView.SetUpgradeBtnInteractable(PlayerManager.Instance.player.CanAfford(p_upgradeTier.upgradeCost));    
            
            int newMax = EditableValuesManager.Instance.GetMaxChaoticEnergyPerPortalLevel(portal.level + 1);
            m_upgradePortalUIView.SetChaoticEnergyUpgradeText(newMax);
            // m_upgradePortalUIView.SetChaoticEnergyUpgradeGOState(true);
            m_upgradePortalUIView.SetChaoticEnergyUpgradeGOState(false);
        }
        m_upgradePortalUIView.PlayShowAnimation();
    }
    private void AnimatedHideUI() {
        m_upgradePortalUIView.PlayHideAnimation(HideUI);
    }
    public override void ShowUI() {
        base.ShowUI();
        isShowing = true;
        m_upgradePortalUIView.SetCurrentSpiritEnergyText(PlayerManager.Instance.player.spiritEnergy);
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        InputManager.Instance.SetSpecificHotkeyEnabledState(KeyCode.Escape, true);
        InnerMapCameraMove.Instance.DisableMovement();
    }
    public override void HideUI() {
        base.HideUI();
        isShowing = false;
        UIManager.Instance.ResumeLastProgressionSpeed();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
        InnerMapCameraMove.Instance.EnableMovement();
    }

    #region Listeners
    private void OnPlayerChosePortalUpgrade() {
        m_upgradePortalUIView.SetUpgradeBtnState(false);
        m_upgradePortalUIView.SetUpgradeTimerState(true);
    }
    private void OnPlayerCancelledPortalUpgrade() {
        m_upgradePortalUIView.SetUpgradeBtnState(true);
        m_upgradePortalUIView.SetUpgradeTimerState(false);
    }
    #endregion
    
    #region UpgradePortalUIView.IListener
    public void OnClickClose() {
        AnimatedHideUI();
    }
    public void OnClickUpgrade() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        PortalUpgradeTier nextTier = portal.nextTier;
        UIManager.Instance.ShowYesNoConfirmation(
            "Portal Upgrade", $"Do you want to spend {nextTier.GetUpgradeCostString()} to upgrade the Portal? ", OnClickConfirmUpgrade, layer:150, showCover: true);
    }

    public void OnClickConfirmUpgrade() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        PortalUpgradeTier nextTier = portal.nextTier;
        portal.PayForUpgrade(nextTier);
        PlayerManager.Instance.player.playerSkillComponent.PlayerStartedPortalUpgrade(nextTier.upgradeCost, nextTier);
        AnimatedHideUI();
        
    }
    public void OnClickCancelUpgrade() {
        UIManager.Instance.ShowYesNoConfirmation(
            "Cancel Portal Upgrade", $"Are you sure you want to cancel Portal Upgrade? " + 
                                     $"\n<i>{UtilityScripts.Utilities.InvalidColorize("Cancelling will reset all current upgrade progress!")}</i>", OnConfirmCancelUpgradePortal, showCover: true, layer: 100);
    }
    public void OnHoverOverCancelUpgrade() {
        UIManager.Instance.ShowSmallInfo(m_tooltipCancelUpgradePortal);
    }
    public void OnHoverOutCancelUpgrade() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverOverUpgradeChaoticEnergy() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        int newMax = EditableValuesManager.Instance.GetMaxChaoticEnergyPerPortalLevel(portal.level + 1);
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails("Chaotic Energy Capacity", $"Increase Chaotic energy capacity to {newMax.ToString()}", position: m_upgradePortalUIView.UIModel.tooltipHoverPos);
    }
    public void OnHoverOutUpgradeChaoticEnergy() {
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    private void OnConfirmCancelUpgradePortal() {
        PlayerManager.Instance.player.playerSkillComponent.CancelPortalUpgrade();
    }
    #endregion

    private void OnHoverOverUpgradeItem(UpgradePortalItemUI p_item) {
        if (PlayerUI.Instance != null) {
            if (p_item.skill != PLAYER_SKILL_TYPE.NONE) {
                PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_item.skill);
                SkillData skillData = PlayerSkillManager.Instance.GetSkillData(playerSkillData.skill);
                PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(skillData.name, skillData.description, position: m_upgradePortalUIView.UIModel.tooltipHoverPos);    
            } else if (p_item.passiveSkill != PASSIVE_SKILL.None) {
                PassiveSkill passiveSkill = PlayerSkillManager.Instance.GetPassiveSkill(p_item.passiveSkill);
                PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(passiveSkill.name, passiveSkill.description, position: m_upgradePortalUIView.UIModel.tooltipHoverPos);
            }
                
        }
    }
    private void OnHoverOutUpgradeItem(UpgradePortalItemUI p_item) {
        if (PlayerUI.Instance != null) {
            PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
        }
    }
    private void OnHoverOverUpgradePortalTimer() {
        string message = $"Remaining time: {PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.GetRemainingTimeString()}";
        UIManager.Instance.ShowSmallInfo(message, autoReplaceText: false);
    }
    private void OnHoverOutUpgradePortalTimer() {
        UIManager.Instance.HideSmallInfo();
    }
    public void HideViaShortcutKey() {
        AnimatedHideUI();
    }
}
