using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class OnDeathUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnIgniteUpgradeClicked();
		void OnWalkerZombieUpgradeClicked();
		void OnMana2_3UpgradeClicked();
		void OnRandomSpirit_1UpgradeClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public OnDeathUIModel UIModel
	{
		get
		{
			return _baseAssetModel as OnDeathUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, OnDeathUIModel p_assets, Action<OnDeathUIView> p_onCreate)
	{
		var go = new GameObject(typeof(OnDeathUIView).ToString());
		var gui = go.AddComponent<OnDeathUIView>();
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
		UIModel.onIgniteUpgradeClicked += p_listener.OnIgniteUpgradeClicked;
		UIModel.onWalkerZombieUpgradeClicked += p_listener.OnWalkerZombieUpgradeClicked;
		UIModel.onMana2_3UpgradeClicked += p_listener.OnMana2_3UpgradeClicked;
		UIModel.onRandomSpirit_1UpgradeClicked += p_listener.OnRandomSpirit_1UpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onIgniteUpgradeClicked -= p_listener.OnIgniteUpgradeClicked;
		UIModel.onWalkerZombieUpgradeClicked -= p_listener.OnWalkerZombieUpgradeClicked;
		UIModel.onMana2_3UpgradeClicked -= p_listener.OnMana2_3UpgradeClicked;
		UIModel.onRandomSpirit_1UpgradeClicked -= p_listener.OnRandomSpirit_1UpgradeClicked;
	}
	#endregion
}