using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class DefensePointUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnDeployClicked();
		void OnCloseClicked();
		void OnAddSummonClicked();
		
		void OnCloseSummonSubContainer();
		void OnHoverOver();
		void OnHoverOut();
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
	
	public void ShowSummonTab() {
		UIModel.scrollViewDeployedSummons.gameObject.SetActive(true);
	}
	public Transform GetAvailableSummonsParent() {
		return UIModel.availableSummonsParent;
	}
		public void EnableDeployButton() {
		UIModel.btnDeploy.interactable = true;
	}

	public void DisableDeployButton() {
		UIModel.btnDeploy.interactable = false;
	}
	#region show/hide Container
	public void ShowSummonSubContainer() {
		HideAllSubMenu();
		UIModel.subSummonContainer.SetActive(true);
	}

	public void ProcessSummonDisplay() {
		int count = 0;
		for (int x = 0; x < UIModel.deployedItemSummonsUI.Count; ++x) {
			if (UIModel.deployedItemSummonsUI[x].isActiveAndEnabled) {
				count++;
			}
		}

		if (count >= 5) {
			UIModel.btnAddSummon.gameObject.SetActive(false);
		} else {
			UIModel.btnAddSummon.gameObject.SetActive(true);
		}
	}

	public void SetTitle(string p_title) {
		UIModel.txtTitle.text = p_title;
	}

	public void SetButtonDeployText(string p_text) {
		UIModel.btnDeploy.GetComponentInChildren<RuinarchText>().text = p_text;
	}

	public void HideAllSubMenu() {
		UIModel.subSummonContainer.SetActive(false);
	}
	#endregion

	public Transform GetDeployedSummonsParent() {
		return UIModel.availableSummonsParent;
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onDeployClicked += p_listener.OnDeployClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onAddSummonClicked += p_listener.OnAddSummonClicked;
		UIModel.onCloseSummonSubContainer += p_listener.OnCloseSummonSubContainer;
		UIModel.onHoverOver += p_listener.OnHoverOver;
		UIModel.onHoverOut += p_listener.OnHoverOut;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onDeployClicked -= p_listener.OnDeployClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onAddSummonClicked -= p_listener.OnAddSummonClicked;
		UIModel.onCloseSummonSubContainer -= p_listener.OnCloseSummonSubContainer;
		UIModel.onHoverOver -= p_listener.OnHoverOver;
		UIModel.onHoverOut -= p_listener.OnHoverOut;
	}
	#endregion
}