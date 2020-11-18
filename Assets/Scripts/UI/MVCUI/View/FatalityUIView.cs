using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class FatalityUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnSepticShockUpgradeClicked();
		void OnHeartAttackUpgradeClicked();
		void OnStrokeUpgradeClicked();
		void OnTotalOrganFailureUpgradeClicked();
		void OnPneumoniaUpgradeClicked();
		void OnSepticShockHoveredOver(UIHoverPosition hoverPosition);
		void OnSepticShockHoveredOut();
		void OnHeartAttackHoveredOver(UIHoverPosition hoverPosition);
		void OnHeartAttackHoveredOut();
		void OnStrokeHoveredOver(UIHoverPosition hoverPosition);
		void OnStrokeHoveredOut();
		void OnTotalOrganFailureHoveredOver(UIHoverPosition hoverPosition);
		void OnTotalOrganFailureHoveredOut();
		void OnPneumoniaHoveredOver(UIHoverPosition hoverPosition);
		void OnPneumoniaHoveredOut();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public FatalityUIModel UIModel
	{
		get
		{
			return _baseAssetModel as FatalityUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, FatalityUIModel p_assets, Action<FatalityUIView> p_onCreate)
	{
		var go = new GameObject(typeof(FatalityUIView).ToString());
		var gui = go.AddComponent<FatalityUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onSepticShockUpgradeClicked += p_listener.OnSepticShockUpgradeClicked;
		UIModel.onHeartAttackUpgradeClicked += p_listener.OnHeartAttackUpgradeClicked;
		UIModel.onStrokeUpgradeClicked += p_listener.OnStrokeUpgradeClicked;
		UIModel.onTotalOrganFailureUpgradeClicked += p_listener.OnTotalOrganFailureUpgradeClicked;
		UIModel.onPneumoniaUpgradeClicked += p_listener.OnPneumoniaUpgradeClicked;
		UIModel.onSepticShockHoveredOver += p_listener.OnSepticShockHoveredOver;
		UIModel.onSepticShockHoveredOut += p_listener.OnSepticShockHoveredOut;
		UIModel.onHeartAttackHoveredOver += p_listener.OnHeartAttackHoveredOver;
		UIModel.onHeartAttackHoveredOut += p_listener.OnHeartAttackHoveredOut;
		UIModel.onStrokeHoveredOver += p_listener.OnStrokeHoveredOver;
		UIModel.onStrokeHoveredOut += p_listener.OnStrokeHoveredOut;
		UIModel.onTotalOrganFailureHoveredOver += p_listener.OnTotalOrganFailureHoveredOver;
		UIModel.onTotalOrganFailureHoveredOut += p_listener.OnTotalOrganFailureHoveredOut;
		UIModel.onPneumoniaHoveredOver += p_listener.OnPneumoniaHoveredOver;
		UIModel.onPneumoniaHoveredOut += p_listener.OnPneumoniaHoveredOut;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onSepticShockUpgradeClicked -= p_listener.OnSepticShockUpgradeClicked;
		UIModel.onHeartAttackUpgradeClicked -= p_listener.OnHeartAttackUpgradeClicked;
		UIModel.onStrokeUpgradeClicked -= p_listener.OnStrokeUpgradeClicked;
		UIModel.onTotalOrganFailureUpgradeClicked -= p_listener.OnTotalOrganFailureUpgradeClicked;
		UIModel.onPneumoniaUpgradeClicked -= p_listener.OnPneumoniaUpgradeClicked;
		UIModel.onSepticShockHoveredOver -= p_listener.OnSepticShockHoveredOver;
		UIModel.onSepticShockHoveredOut -= p_listener.OnSepticShockHoveredOut;
		UIModel.onHeartAttackHoveredOver -= p_listener.OnHeartAttackHoveredOver;
		UIModel.onHeartAttackHoveredOut -= p_listener.OnHeartAttackHoveredOut;
		UIModel.onStrokeHoveredOver -= p_listener.OnStrokeHoveredOver;
		UIModel.onStrokeHoveredOut -= p_listener.OnStrokeHoveredOut;
		UIModel.onTotalOrganFailureHoveredOver -= p_listener.OnTotalOrganFailureHoveredOver;
		UIModel.onTotalOrganFailureHoveredOut -= p_listener.OnTotalOrganFailureHoveredOut;
		UIModel.onPneumoniaHoveredOver -= p_listener.OnPneumoniaHoveredOver;
		UIModel.onPneumoniaHoveredOut -= p_listener.OnPneumoniaHoveredOut;
	}
	#endregion

	public void UpdateSepticShockUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnSepticShockUpgrade.interactable = p_interactable;
		UIModel.txtSepticShockUpgrade.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
	public void UpdateSepticShockCostState(bool p_state) {
		UIModel.txtSepticShockCost.gameObject.SetActive(p_state);
	}
	public void UpdateSepticShockCost(string p_cost) {
		UIModel.txtSepticShockCost.text = p_cost;
	}
	
	public void UpdateHeartAttackUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnHeartAttackUpgrade.interactable = p_interactable;
		UIModel.txtHeartAttackUpgrade.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
	public void UpdateHeartAttackCostState(bool p_state) {
		UIModel.txtHeartAttackCost.gameObject.SetActive(p_state);
	}
	public void UpdateHeartAttackCost(string p_cost) {
		UIModel.txtHeartAttackCost.text = p_cost;
	}
	
	public void UpdateStrokeUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnStrokeUpgrade.interactable = p_interactable;
		UIModel.txtStrokeUpgrade.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
	public void UpdateStrokeCostState(bool p_state) {
		UIModel.txtStrokeCost.gameObject.SetActive(p_state);
	}
	public void UpdateStrokeCost(string p_cost) {
		UIModel.txtStrokeCost.text = p_cost;
	}
	
	public void UpdateTotalOrganFailureUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnTotalOrganFailureUpgrade.interactable = p_interactable;
		UIModel.txtTotalOrganFailureUpgrade.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
	public void UpdateTotalOrganFailureCostState(bool p_state) {
		UIModel.txtTotalOrganFailureCost.gameObject.SetActive(p_state);
	}
	public void UpdateTotalOrganFailureCost(string p_cost) {
		UIModel.txtTotalOrganFailureCost.text = p_cost;
	}
	
	public void UpdatePneumoniaUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnPneumoniaUpgrade.interactable = p_interactable;
		UIModel.txtPneumoniaUpgrade.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
	public void UpdatePneumoniaCostState(bool p_state) {
		UIModel.txtPneumoniaCost.gameObject.SetActive(p_state);
	}
	public void UpdatePneumoniaCost(string p_cost) {
		UIModel.txtPneumoniaCost.text = p_cost;
	}
}