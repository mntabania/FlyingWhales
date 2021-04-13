using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Plague.Transmission;

public class TransmissionUIController : MVCUIController, TransmissionUIView.IListener
{
	[SerializeField]
	private TransmissionUIModel m_transmissionUIModel;
	private TransmissionUIView m_transmissionUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		TransmissionUIView.Create(_canvas, m_transmissionUIModel, (p_ui) => {
			m_transmissionUIView = p_ui;
			m_transmissionUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}
	public override void ShowUI() {
		base.ShowUI();
		UpdateAllTransmissionData();
	}

	#region TransmissionUIView.IListener implementation
	public void OnAirBorneUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Airborne);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Airborne);
		UpdateAllTransmissionData();
	}
	public void OnConsumptionUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Consumption);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Consumption);
		UpdateAllTransmissionData();
	}
	
	public void OnPhysicalContactUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Physical_Contact);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact);
		UpdateAllTransmissionData();
	}
	public void OnCombatUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Combat);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Combat);
		UpdateAllTransmissionData();
	}
	public void OnAirBorneHoveredOver(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_TRANSMISSION.Airborne, p_hoverPosition); }
	public void OnConsumptionHoveredOver(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_TRANSMISSION.Consumption, p_hoverPosition); }
	public void OnPhysicalContactHoveredOver(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_TRANSMISSION.Physical_Contact, p_hoverPosition); }
	public void OnCombatHoveredOver(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_TRANSMISSION.Combat, p_hoverPosition); }
	public void OnAirBorneHoveredOut() { HideTooltip(); }
	public void OnConsumptionHoveredOut() { HideTooltip(); }
	public void OnPhysicalContactHoveredOut() { HideTooltip(); }
	public void OnCombatHoveredOut() { HideTooltip(); }
	#endregion
	
	private void ShowTooltip(PLAGUE_TRANSMISSION p_transmissionType, UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null) { UIManager.Instance.ShowSmallInfo(p_transmissionType.GetTransmissionTooltip(), p_hoverPosition, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_transmissionType.ToString())); }
	}
	private void HideTooltip() {
		if (UIManager.Instance != null) { UIManager.Instance.HideSmallInfo(); }
	}
	
	private void UpdateAllTransmissionData() {
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Airborne);
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Consumption);
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Physical_Contact);
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Combat);
	}
	 
	private void UpdateTransmissionData(PLAGUE_TRANSMISSION p_transmissionType) {
		int nextLevelCost = GetTransmissionUpgradeCost(p_transmissionType);
		m_transmissionUIView.UpdateTransmissionRate(p_transmissionType, PlagueDisease.Instance.GetTransmissionRateDescription(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)));
		bool isMaxLevel = PlagueDisease.Instance.IsMaxLevel(p_transmissionType);
		bool canTransmissionBeUpgraded = !isMaxLevel && (!PlagueDisease.Instance.HasMaxTransmissions() || PlagueDisease.Instance.IsTransmissionActive(p_transmissionType));
		m_transmissionUIView.UpdateTransmissionUpgradeButtonInteractable(p_transmissionType, canTransmissionBeUpgraded && CanAffordUpgrade(p_transmissionType));
		m_transmissionUIView.UpdateTransmissionCost(p_transmissionType, isMaxLevel ? "MAX" : $"{nextLevelCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		// m_transmissionUIView.UpdateTransmissionCostPlagueIcon(p_transmissionType, !isMaxLevel);
	}
	private int GetTransmissionUpgradeCost(PLAGUE_TRANSMISSION p_transmissionType) {
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				return AirborneTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			case PLAGUE_TRANSMISSION.Consumption:
				return ConsumptionTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption));
			case PLAGUE_TRANSMISSION.Physical_Contact:
				return PhysicalContactTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact));
			case PLAGUE_TRANSMISSION.Combat:
				return CombatRateTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat));
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
	}
	

	private void PayForUpgrade(PLAGUE_TRANSMISSION p_transmissionType) {
		int upgradeCost;
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				upgradeCost = AirborneTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Consumption:
				upgradeCost = ConsumptionTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				upgradeCost = PhysicalContactTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Combat:
				upgradeCost = CombatRateTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
	}
	private bool CanAffordUpgrade(PLAGUE_TRANSMISSION p_transmissionType) {
		int upgradeCost;
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				upgradeCost = AirborneTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Consumption:
				upgradeCost = ConsumptionTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				upgradeCost = PhysicalContactTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Combat:
				upgradeCost = CombatRateTransmission.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= upgradeCost || (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None);
		} else {
			return true;
		}
	}
}