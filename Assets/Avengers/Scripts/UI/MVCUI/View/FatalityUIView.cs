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
	public void Subscribe(IListener p_listener)
	{
		UIModel.onSepticShockUpgradeClicked += p_listener.OnSepticShockUpgradeClicked;
		UIModel.onHeartAttackUpgradeClicked += p_listener.OnHeartAttackUpgradeClicked;
		UIModel.onStrokeUpgradeClicked += p_listener.OnStrokeUpgradeClicked;
		UIModel.onTotalOrganFailureUpgradeClicked += p_listener.OnTotalOrganFailureUpgradeClicked;
		UIModel.onPneumoniaUpgradeClicked += p_listener.OnPneumoniaUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onSepticShockUpgradeClicked -= p_listener.OnSepticShockUpgradeClicked;
		UIModel.onHeartAttackUpgradeClicked -= p_listener.OnHeartAttackUpgradeClicked;
		UIModel.onStrokeUpgradeClicked -= p_listener.OnStrokeUpgradeClicked;
		UIModel.onTotalOrganFailureUpgradeClicked -= p_listener.OnTotalOrganFailureUpgradeClicked;
		UIModel.onPneumoniaUpgradeClicked -= p_listener.OnPneumoniaUpgradeClicked;
	}
	#endregion
}