using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class TransmissionUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnAirBorneUpgradeClicked();
		void OnConsumptionUpgradeClicked();
		void OnPhysicalContactUpgradeClicked();
		void OnCombatUpgradeClicked();
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

	public void UpdateAirbornePrice(string p_newPrice) {
		UIModel.txtAirBorneCost.text = p_newPrice;
	}
	public void UpdateAirborneRate(string p_rate) {
		UIModel.txtAirBorneRate.text = p_rate;
	}
	public void UpdateAirborneRateButtonInteractable(bool p_interactable) {
		UIModel.btnAirBorneUpgrade.interactable = p_interactable;
	}
	public void UpdateAirbornePriceState(bool p_state) {
		UIModel.txtAirBorneCost.gameObject.SetActive(p_state);
	}

	public void UpdateConsumptionPrice(string p_newPrice) {
		UIModel.txtConsumptionCost.text = p_newPrice;
	}
	public void UpdateConsumptionRate(string p_rate) {
		UIModel.txtConsumptionRate.text = p_rate;
	}
	public void UpdateConsumptionButtonInteractable(bool p_interactable) {
		UIModel.btnConsumptionUpgrade.interactable = p_interactable;
	}
	public void UpdateConsumptionPriceState(bool p_state) {
		UIModel.txtConsumptionCost.gameObject.SetActive(p_state);
	}

	public void UpdatePhysicalContactPrice(string p_newPrice) {
		UIModel.txtDirectContactCost.text = p_newPrice;
	}
	public void UpdatePhysicalContactRate(string p_rate) {
		UIModel.txtDirectContactRate.text = p_rate;
	}
	public void UpdatePhysicalContactRateButtonInteractable(bool p_interactable) {
		UIModel.btnDirectContactUpgrade.interactable = p_interactable;
	}
	public void UpdatePhysicalContactPriceState(bool p_state) {
		UIModel.txtDirectContactCost.gameObject.SetActive(p_state);
	}
	
	public void UpdateCombatPrice(string p_newPrice) {
		UIModel.txtCombatUpgradeCost.text = p_newPrice;
	}
	public void UpdateCombatRate(string p_rate) {
		UIModel.txtCombatRate.text = p_rate;
	}
	public void UpdateCombatRateButtonInteractable(bool p_interactable) {
		UIModel.btnCombatUpgrade.interactable = p_interactable;
	}
	public void UpdateCombatRatePriceState(bool p_state) {
		UIModel.txtCombatUpgradeCost.gameObject.SetActive(p_state);
	}


	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.onAirBorneUpgradeClicked += p_listener.OnAirBorneUpgradeClicked;
		UIModel.onConsumptionUpgradeClicked += p_listener.OnConsumptionUpgradeClicked;
		UIModel.onDirectContactUpgradeClicked += p_listener.OnPhysicalContactUpgradeClicked;
		UIModel.onCombatUpgradeClicked += p_listener.OnCombatUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onAirBorneUpgradeClicked -= p_listener.OnAirBorneUpgradeClicked;
		UIModel.onConsumptionUpgradeClicked -= p_listener.OnConsumptionUpgradeClicked;
		UIModel.onDirectContactUpgradeClicked -= p_listener.OnPhysicalContactUpgradeClicked;
		UIModel.onCombatUpgradeClicked -= p_listener.OnCombatUpgradeClicked;
	}
	#endregion
}