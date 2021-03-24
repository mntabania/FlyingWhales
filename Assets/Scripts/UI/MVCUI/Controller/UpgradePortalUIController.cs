using System;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UpgradePortalUIController : MVCUIController, UpgradePortalUIView.IListener {
    [SerializeField]
    private UpgradePortalUIModel m_upgradePortalUIModel;
    private UpgradePortalUIView m_upgradePortalUIView;
    
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
    }
    public void ShowPortalUpgradeTier(PortalUpgradeTier p_upgradeTier, int p_level) {
        ShowUI();
        m_upgradePortalUIView.UpdateItems(p_upgradeTier);
        m_upgradePortalUIView.SetHeader($"Upgrade to Level {(p_level + 1).ToString()}?");
        m_upgradePortalUIView.SetUpgradeText($"Upgrade - {p_upgradeTier.GetUpgradeCostString()}");
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
            m_upgradePortalUIView.SetUpgradeBtnInteractable(PlayerManager.Instance.player.CanAfford(p_upgradeTier.upgradeCost));    
        }
        m_upgradePortalUIView.PlayShowAnimation();
    }
    public void AnimatedHideUI() {
        m_upgradePortalUIView.PlayHideAnimation(HideUI);
    }

    #region UpgradePortalUIView.IListener
    public void OnClickClose() {
        AnimatedHideUI();
    }
    public void OnClickUpgrade() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        PortalUpgradeTier nextTier = portal.nextTier;
        portal.PayForUpgrade(nextTier);
        PlayerManager.Instance.player.playerSkillComponent.PlayerStartedPortalUpgrade(nextTier.upgradeCost, nextTier);
        AnimatedHideUI();
    }
    #endregion

    private void OnHoverOverUpgradeItem(UpgradePortalItemUI p_item) {
        if (PlayerUI.Instance != null) {
            SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_item.skill);
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(skillData, m_upgradePortalUIView.UIModel.tooltipHoverPos);    
        }
    }
    private void OnHoverOutUpgradeItem(UpgradePortalItemUI p_item) {
        if (PlayerUI.Instance != null) {
            PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
        }
    }
    
}
