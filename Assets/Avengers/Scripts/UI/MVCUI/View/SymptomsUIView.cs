using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class SymptomsUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnParalysisUpgradeClicked();
		void OnVomitingUpgradeClicked();
		void OnLethargyUpgradeClicked();
		void OnSeizuresUpgradeClicked();
		void OnInsomniaUpgradeClicked();
		void OnPoisonCloudUpgradeClicked();
		void OnMonsterScenetUpgradeClicked();
		void OnSneezingUpgradeClicked();
		void OnDepressionUpgradeClicked();
		void OnHungerPangsUpgradeClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public SymptomsUIModel UIModel
	{
		get
		{
			return _baseAssetModel as SymptomsUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, SymptomsUIModel p_assets, Action<SymptomsUIView> p_onCreate)
	{
		var go = new GameObject(typeof(SymptomsUIView).ToString());
		var gui = go.AddComponent<SymptomsUIView>();
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
		UIModel.onParalysisUpgradeClicked += p_listener.OnParalysisUpgradeClicked;
		UIModel.onVomitingUpgradeClicked += p_listener.OnVomitingUpgradeClicked;
		UIModel.onLethargyUpgradeClicked += p_listener.OnLethargyUpgradeClicked;
		UIModel.onSeizuresUpgradeClicked += p_listener.OnSeizuresUpgradeClicked;
		UIModel.onInsomniaUpgradeClicked += p_listener.OnInsomniaUpgradeClicked;
		UIModel.onPoisonCloudUpgradeClicked += p_listener.OnPoisonCloudUpgradeClicked;
		UIModel.onMonsterScentUpgradeClicked += p_listener.OnMonsterScenetUpgradeClicked;
		UIModel.onSneezingUpgradeClicked += p_listener.OnSneezingUpgradeClicked;
		UIModel.onDepressionUpgradeClicked += p_listener.OnDepressionUpgradeClicked;
		UIModel.onHungerPangsUpgradeClicked += p_listener.OnHungerPangsUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onParalysisUpgradeClicked -= p_listener.OnParalysisUpgradeClicked;
		UIModel.onVomitingUpgradeClicked -= p_listener.OnVomitingUpgradeClicked;
		UIModel.onLethargyUpgradeClicked -= p_listener.OnLethargyUpgradeClicked;
		UIModel.onSeizuresUpgradeClicked -= p_listener.OnSeizuresUpgradeClicked;
		UIModel.onInsomniaUpgradeClicked -= p_listener.OnInsomniaUpgradeClicked;
		UIModel.onPoisonCloudUpgradeClicked -= p_listener.OnPoisonCloudUpgradeClicked;
		UIModel.onMonsterScentUpgradeClicked -= p_listener.OnMonsterScenetUpgradeClicked;
		UIModel.onSneezingUpgradeClicked -= p_listener.OnSneezingUpgradeClicked;
		UIModel.onDepressionUpgradeClicked -= p_listener.OnDepressionUpgradeClicked;
		UIModel.onHungerPangsUpgradeClicked -= p_listener.OnHungerPangsUpgradeClicked;
	}
	#endregion
}