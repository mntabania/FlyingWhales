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
		void OnDirectContactUpgradeClicked();
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

	public void UpdateAirbornePrice(string p_newPrice) 
	{
		UIModel.txtAirBorneCost.text = p_newPrice;
	}

	public void UpdateConsumptionPrice(string p_newPrice)
	{
		UIModel.txtConsumptionCost.text = p_newPrice;
	}

	public void UpdateDirectContactPrice(string p_newPrice)
	{
		UIModel.txtDirectContactCost.text = p_newPrice;
	}

	public void UpdateCombatPrice(string p_newPrice)
	{
		UIModel.txtCombatUpgradeCost.text = p_newPrice;
	}


	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.onAirBorneUpgradeClicked += p_listener.OnAirBorneUpgradeClicked;
		UIModel.onConsumptionUpgradeClicked += p_listener.OnConsumptionUpgradeClicked;
		UIModel.onDirectContactUpgradeClicked += p_listener.OnDirectContactUpgradeClicked;
		UIModel.onCombatUpgradeClicked += p_listener.OnCombatUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onAirBorneUpgradeClicked -= p_listener.OnAirBorneUpgradeClicked;
		UIModel.onConsumptionUpgradeClicked -= p_listener.OnConsumptionUpgradeClicked;
		UIModel.onDirectContactUpgradeClicked -= p_listener.OnDirectContactUpgradeClicked;
		UIModel.onCombatUpgradeClicked -= p_listener.OnCombatUpgradeClicked;
	}
	#endregion
}