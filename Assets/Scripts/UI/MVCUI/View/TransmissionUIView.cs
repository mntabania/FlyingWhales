using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using UnityEngine.UI;

public class TransmissionUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnAirBorneUpgradeClicked();
		void OnConsumptionUpgradeClicked();
		void OnPhysicalContactUpgradeClicked();
		void OnCombatUpgradeClicked();
		void OnAirBorneHoveredOver(UIHoverPosition p_hoverPosition);
		void OnConsumptionHoveredOver(UIHoverPosition p_hoverPosition);
		void OnPhysicalContactHoveredOver(UIHoverPosition p_hoverPosition);
		void OnCombatHoveredOver(UIHoverPosition p_hoverPosition);
		void OnAirBorneHoveredOut();
		void OnConsumptionHoveredOut();
		void OnPhysicalContactHoveredOut();
		void OnCombatHoveredOut();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public TransmissionUIModel UIModel
	{
		get
		{
			return _baseAssetModel as TransmissionUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, TransmissionUIModel p_assets, Action<TransmissionUIView> p_onCreate)
	{
		var go = new GameObject(typeof(TransmissionUIView).ToString());
		var gui = go.AddComponent<TransmissionUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	private RuinarchText GetTransmissionCostText(PLAGUE_TRANSMISSION p_transmissionType) {
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				return UIModel.txtAirBorneCost;
			case PLAGUE_TRANSMISSION.Consumption:
				return UIModel.txtConsumptionCost;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				return UIModel.txtDirectContactCost;
			case PLAGUE_TRANSMISSION.Combat:
				return UIModel.txtCombatUpgradeCost;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
	}
	private RuinarchText GetTransmissionRateText(PLAGUE_TRANSMISSION p_transmissionType) {
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				return UIModel.txtAirBorneRate;
			case PLAGUE_TRANSMISSION.Consumption:
				return UIModel.txtConsumptionRate;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				return UIModel.txtDirectContactRate;
			case PLAGUE_TRANSMISSION.Combat:
				return UIModel.txtCombatRate;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
	}
	private Button GetTransmissionUpgradeButton(PLAGUE_TRANSMISSION p_transmissionType) {
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				return UIModel.btnAirBorneUpgrade;
			case PLAGUE_TRANSMISSION.Consumption:
				return UIModel.btnConsumptionUpgrade;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				return UIModel.btnDirectContactUpgrade;
			case PLAGUE_TRANSMISSION.Combat:
				return UIModel.btnCombatUpgrade;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
	}
	private GameObject GetCostPlagueIcon(PLAGUE_TRANSMISSION p_transmissionType) {
		switch (p_transmissionType) {
			case PLAGUE_TRANSMISSION.Airborne:
				return UIModel.airBorneCostImagePlagueIcon;
			case PLAGUE_TRANSMISSION.Consumption:
				return UIModel.consumptionCostPlagueIcon;
			case PLAGUE_TRANSMISSION.Physical_Contact:
				return UIModel.directContactCostPlagueIcon;
			case PLAGUE_TRANSMISSION.Combat:
				return UIModel.combatCostPlagueIcon;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
		}
	}

	public void UpdateTransmissionCost(PLAGUE_TRANSMISSION p_transmissionType, string p_newCost) {
		RuinarchText txtCost = GetTransmissionCostText(p_transmissionType);
		txtCost.text = p_newCost;
	}
	public void UpdateTransmissionCostPlagueIcon(PLAGUE_TRANSMISSION p_transmissionType, bool state) {
		GameObject iconGO = GetCostPlagueIcon(p_transmissionType);
		iconGO.SetActive(state);
	}
	public void UpdateTransmissionRate(PLAGUE_TRANSMISSION p_transmissionType, string p_newRate) {
		RuinarchText txtRate = GetTransmissionRateText(p_transmissionType);
		txtRate.text = p_newRate;
	}
	public void UpdateTransmissionUpgradeButtonInteractable(PLAGUE_TRANSMISSION p_transmissionType, bool p_state) {
		Button button = GetTransmissionUpgradeButton(p_transmissionType);
		button.interactable = p_state;
	}


	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.onAirBorneUpgradeClicked += p_listener.OnAirBorneUpgradeClicked;
		UIModel.onConsumptionUpgradeClicked += p_listener.OnConsumptionUpgradeClicked;
		UIModel.onDirectContactUpgradeClicked += p_listener.OnPhysicalContactUpgradeClicked;
		UIModel.onCombatUpgradeClicked += p_listener.OnCombatUpgradeClicked;
		UIModel.onAirBorneHoveredOver += p_listener.OnAirBorneHoveredOver;
		UIModel.onConsumptionHoveredOver += p_listener.OnConsumptionHoveredOver;
		UIModel.onDirectContactHoveredOver += p_listener.OnPhysicalContactHoveredOver;
		UIModel.onCombatHoveredOver += p_listener.OnCombatHoveredOver;
		UIModel.onAirBorneHoveredOut += p_listener.OnAirBorneHoveredOut;
		UIModel.onConsumptionHoveredOut += p_listener.OnConsumptionHoveredOut;
		UIModel.onDirectContactHoveredOut += p_listener.OnPhysicalContactHoveredOut;
		UIModel.onCombatHoveredOut += p_listener.OnCombatHoveredOut;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onAirBorneUpgradeClicked -= p_listener.OnAirBorneUpgradeClicked;
		UIModel.onConsumptionUpgradeClicked -= p_listener.OnConsumptionUpgradeClicked;
		UIModel.onDirectContactUpgradeClicked -= p_listener.OnPhysicalContactUpgradeClicked;
		UIModel.onCombatUpgradeClicked -= p_listener.OnCombatUpgradeClicked;
		UIModel.onAirBorneHoveredOver -= p_listener.OnAirBorneHoveredOver;
		UIModel.onConsumptionHoveredOver -= p_listener.OnConsumptionHoveredOver;
		UIModel.onDirectContactHoveredOver -= p_listener.OnPhysicalContactHoveredOver;
		UIModel.onCombatHoveredOver -= p_listener.OnCombatHoveredOver;
		UIModel.onAirBorneHoveredOut -= p_listener.OnAirBorneHoveredOut;
		UIModel.onConsumptionHoveredOut -= p_listener.OnConsumptionHoveredOut;
		UIModel.onDirectContactHoveredOut -= p_listener.OnPhysicalContactHoveredOut;
		UIModel.onCombatHoveredOut -= p_listener.OnCombatHoveredOut;
	}
	#endregion
}