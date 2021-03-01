using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class MaraudUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnDeployClicked();
		void OnCloseClicked();
		void OnSummonsClicked(bool isOn);
		void OnMinionClicked(bool isOn);
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public MaraudUIModel UIModel {
		get {
			return _baseAssetModel as MaraudUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, MaraudUIModel p_assets, Action<MaraudUIView> p_onCreate) {
		var go = new GameObject(typeof(MaraudUIView).ToString());
		var gui = go.AddComponent<MaraudUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null) {
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region user defined functions
	public void ShowMinionTab() {
		UIModel.scrollViewMinions.gameObject.SetActive(true);
		UIModel.scrollViewSummons.gameObject.SetActive(false);
	}

	public void ShowSummonTab() {
		UIModel.scrollViewMinions.gameObject.SetActive(false);
		UIModel.scrollViewSummons.gameObject.SetActive(true);
	}
	public Transform GetAvailableSummonsParent() {
		return UIModel.availableSummonsParent;
	}

	public Transform GetAvailableMinionsParent() {
		return UIModel.availableMinionsParent;
	}
	public Transform GetDeployedMonsterParent() {
		return UIModel.deplyedMonstersParent;
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onDeployClicked += p_listener.OnDeployClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onMinionClicked += p_listener.OnMinionClicked;
		UIModel.onSummonClicked += p_listener.OnSummonsClicked;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onDeployClicked -= p_listener.OnDeployClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onMinionClicked -= p_listener.OnMinionClicked;
		UIModel.onSummonClicked -= p_listener.OnSummonsClicked;
	}
	#endregion
}