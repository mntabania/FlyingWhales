using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class DefensePointUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnDeployClicked();
		void OnCloseClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public DefensePointUIModel UIModel {
		get {
			return _baseAssetModel as DefensePointUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, DefensePointUIModel p_assets, Action<DefensePointUIView> p_onCreate) {
		var go = new GameObject(typeof(DefensePointUIView).ToString());
		var gui = go.AddComponent<DefensePointUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null) {
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region user defined functions

	public Transform GetAvailableMonsterParent() {
		return UIModel.availableMonstersParent;
	}
	public Transform GetDeployedMonsterParent() {
		return UIModel.deplyedMonstersParent;
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onDeployClicked += p_listener.OnDeployClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onDeployClicked -= p_listener.OnDeployClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
	}
	#endregion
}