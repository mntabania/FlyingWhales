using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class MaraudUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnDeployClicked();
		void OnCloseClicked();
		void OnAddSummonClicked();
		void OnAddMinionClicked();
		void OnAddTargetClicked();
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
		UIModel.scrollViewDeployedMinions.gameObject.SetActive(true);
		UIModel.scrollViewDeployedSummons.gameObject.SetActive(false);
	}

	public void ShowSummonTab() {
		UIModel.scrollViewDeployedMinions.gameObject.SetActive(false);
		UIModel.scrollViewDeployedSummons.gameObject.SetActive(true);
	}
	public Transform GetAvailableSummonsParent() {
		return UIModel.availableSummonsParent;
	}

	#region show/hide Container
	public void ShowSummonSubContainer() {
		HideAllSubMenu();
		UIModel.subSummonContainer.SetActive(true);
	}

	public void ShowMinionSubContainer() {
		HideAllSubMenu();
		UIModel.subMinionContainer.SetActive(true);
	}

	public void ShowTargetSubContainer() {
		HideAllSubMenu();
		UIModel.subTargetContainer.SetActive(true);
	}

	public void HideMinionButtonShowMinionContainer() {
		UIModel.btnAddMinion.gameObject.SetActive(false);
		UIModel.scrollViewDeployedMinions.gameObject.SetActive(true);
	}

	public void ShowMinionButtonHideMinionContainer() {
		UIModel.btnAddMinion.gameObject.SetActive(true);
		UIModel.scrollViewDeployedMinions.gameObject.SetActive(false);
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

	public void HideTargetButtonShowTargetContainer() {
		UIModel.btnAddMinion.gameObject.SetActive(false);
		UIModel.scrollViewDeployedMinions.gameObject.SetActive(true);
	}

	public void ShowTargetButtonHideTargetContainer() {
		UIModel.btnAddMinion.gameObject.SetActive(true);
		UIModel.scrollViewDeployedMinions.gameObject.SetActive(false);
	}

	public void SetTitle(string p_title) {
		UIModel.txtTitle.text = p_title;
	}

	public void HideAllSubMenu() {
		UIModel.subMinionContainer.SetActive(false);
		UIModel.subSummonContainer.SetActive(false);
		UIModel.subTargetContainer.SetActive(false);
	}
	#endregion

	public Transform GetAvailableMinionsParent() {
		return UIModel.availableMinionsParent;
	}
	public Transform GetDeployedMinionsParent() {
		return UIModel.deployedMinionsParent;
	}
	public Transform GetDeployedSummonsParent() {
		return UIModel.deployedSummonsParent;
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onDeployClicked += p_listener.OnDeployClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onAddMinionClicked += p_listener.OnAddMinionClicked;
		UIModel.onAddSummonClicked += p_listener.OnAddSummonClicked;
		UIModel.onAddTargetClicked += p_listener.OnAddTargetClicked;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onDeployClicked -= p_listener.OnDeployClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onAddMinionClicked -= p_listener.OnAddMinionClicked;
		UIModel.onAddSummonClicked -= p_listener.OnAddSummonClicked;
		UIModel.onAddTargetClicked -= p_listener.OnAddTargetClicked;
	}
	#endregion
}