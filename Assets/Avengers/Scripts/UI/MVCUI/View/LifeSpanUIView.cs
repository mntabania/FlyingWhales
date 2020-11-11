using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class LifeSpanUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnObjectsUpgradeClicked();
		void OnElvesUpgradeClicked();
		void OnHumansUpgradeClicked();
		void OnMonstersUpgradeClicked();
		void OnUndeadUpgradeClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public LifeSpanUIModel UIModel
	{
		get
		{
			return _baseAssetModel as LifeSpanUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, LifeSpanUIModel p_assets, Action<LifeSpanUIView> p_onCreate)
	{
		var go = new GameObject(typeof(LifeSpanUIView).ToString());
		var gui = go.AddComponent<LifeSpanUIView>();
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
		UIModel.onObjectsUpgradeClicked += p_listener.OnObjectsUpgradeClicked;
		UIModel.onElvesUpgradeClicked += p_listener.OnElvesUpgradeClicked;
		UIModel.onHumansUpgradeClicked += p_listener.OnHumansUpgradeClicked;
		UIModel.onMonstersUpgradeClicked += p_listener.OnMonstersUpgradeClicked;
		UIModel.onUndeadUpgradeClicked += p_listener.OnUndeadUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onObjectsUpgradeClicked -= p_listener.OnObjectsUpgradeClicked;
		UIModel.onElvesUpgradeClicked -= p_listener.OnElvesUpgradeClicked;
		UIModel.onHumansUpgradeClicked -= p_listener.OnHumansUpgradeClicked;
		UIModel.onMonstersUpgradeClicked -= p_listener.OnMonstersUpgradeClicked;
		UIModel.onUndeadUpgradeClicked -= p_listener.OnUndeadUpgradeClicked;
	}
	#endregion
}