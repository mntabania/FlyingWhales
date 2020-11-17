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
		UpdateTransmissionData();
	}
	private void UpdateTransmissionData() {
		UpdateAirborneTransmissionData();
		UpdateConsumptionTransmissionData();
		UpdatePhysicalContactTransmissionData();
		UpdateCombatTransmissionData();
	}
	private void UpdateAirborneTransmissionData() {
		m_transmissionUIView.UpdateAirbornePrice(AirborneTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne)).ToString());
		m_transmissionUIView.UpdateAirborneRate(PlagueDisease.Instance.GetTransmissionRateDescription(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne)));
		bool isMaxLevel = PlagueDisease.Instance.IsMaxLevel(PLAGUE_TRANSMISSION.Airborne);
		m_transmissionUIView.UpdateAirborneRateButtonInteractable(!isMaxLevel && CanAffordUpgrade(PLAGUE_TRANSMISSION.Airborne));
		m_transmissionUIView.UpdateAirbornePriceState(!isMaxLevel);
	}
	private void UpdateConsumptionTransmissionData() {
		m_transmissionUIView.UpdateConsumptionPrice(ConsumptionTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption)).ToString());
		m_transmissionUIView.UpdateConsumptionRate(PlagueDisease.Instance.GetTransmissionRateDescription(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption)));
		bool isMaxLevel = PlagueDisease.Instance.IsMaxLevel(PLAGUE_TRANSMISSION.Consumption);
		m_transmissionUIView.UpdateConsumptionButtonInteractable(!isMaxLevel && CanAffordUpgrade(PLAGUE_TRANSMISSION.Consumption));
		m_transmissionUIView.UpdateConsumptionPriceState(!isMaxLevel);
	}
	private void UpdatePhysicalContactTransmissionData() {
		m_transmissionUIView.UpdatePhysicalContactPrice(PhysicalContactTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact)).ToString());
		m_transmissionUIView.UpdatePhysicalContactRate(PlagueDisease.Instance.GetTransmissionRateDescription(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact)));
		bool isMaxLevel = PlagueDisease.Instance.IsMaxLevel(PLAGUE_TRANSMISSION.Physical_Contact);
		m_transmissionUIView.UpdatePhysicalContactRateButtonInteractable(!isMaxLevel && CanAffordUpgrade(PLAGUE_TRANSMISSION.Physical_Contact));
		m_transmissionUIView.UpdatePhysicalContactPriceState(!isMaxLevel);
	}
	private void UpdateCombatTransmissionData() {
		m_transmissionUIView.UpdateCombatPrice(CombatRateTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat)).ToString());
		m_transmissionUIView.UpdateCombatRate(PlagueDisease.Instance.GetTransmissionRateDescription(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat)));
		bool isMaxLevel = PlagueDisease.Instance.IsMaxLevel(PLAGUE_TRANSMISSION.Combat);
		m_transmissionUIView.UpdateCombatRateButtonInteractable(!isMaxLevel && CanAffordUpgrade(PLAGUE_TRANSMISSION.Combat));
		m_transmissionUIView.UpdateCombatRatePriceState(!isMaxLevel);
	}

	#region TransmissionUIView.IListener implementation
	public void OnAirBorneUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Airborne);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Airborne);
		UpdateAirborneTransmissionData();
	}
	public void OnConsumptionUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Consumption);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Consumption);
		UpdateConsumptionTransmissionData();
	}
	
	public void OnPhysicalContactUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Physical_Contact);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact);
		UpdatePhysicalContactTransmissionData();
	}
	public void OnCombatUpgradeClicked() {
		PayForUpgrade(PLAGUE_TRANSMISSION.Combat);
		PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Combat);
		UpdateCombatTransmissionData();
	}
	#endregion

	private void PayForUpgrade(PLAGUE_TRANSMISSION p_transmissionType) {
		int upgradeCost;
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				upgradeCost = AirborneTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Consumption:
				upgradeCost = ConsumptionTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				upgradeCost = PhysicalContactTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Combat:
				upgradeCost = CombatRateTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
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
				upgradeCost = AirborneTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Consumption:
				upgradeCost = ConsumptionTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				upgradeCost = PhysicalContactTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			case PLAGUE_TRANSMISSION.Combat:
				upgradeCost = CombatRateTransmission.Instance.GetTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType));
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= upgradeCost;
		} else {
			return true;
		}
	}
}