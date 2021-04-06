using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class MaraudUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnDeployClicked();
		void OnCloseClicked();
		void OnAddMinionClicked();
		void OnAddTargetClicked();
		void OnCloseSummonSubContainer();
		void OnCloseMinionSubContainer();
		void OnCloseTargetSubContainer();
		void OnHoverOver();
		void OnHoverOut();
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

	public void HideTargetButtonShowTargetContainer() {
		UIModel.btnAddTarget.gameObject.SetActive(false);
		UIModel.scrollViewDeployedTargets.gameObject.SetActive(true);
	}

	public void ShowTargetButtonHideTargetContainer() {
		UIModel.btnAddTarget.gameObject.SetActive(true);
		UIModel.scrollViewDeployedTargets.gameObject.SetActive(false);
	}

	public void ProcessSummonDisplay(int p_currentCount, int p_maxCount, int p_currentMana) {
		int lastAvailIndex = -1;
		for (int x = 0; x < p_currentCount; ++x) {
			if (!UIModel.deployedItemSummonsUI[x].isDeployed && !UIModel.deployedItemSummonsUI[x].isReadyForDeploy) {
				UIModel.deployedItemSummonsUI[x].gameObject.SetActive(true);
				UIModel.deployedItemSummonsUI[x].MakeSlotEmpty();
				if (lastAvailIndex == -1) {
					lastAvailIndex = x;
				}
			}
		}

		if (lastAvailIndex != -1) {
			UIModel.deployedItemSummonsUI[lastAvailIndex].gameObject.SetActive(true);
			UIModel.deployedItemSummonsUI[lastAvailIndex].DisplayAddSummon();
		}
		if (p_currentCount < p_maxCount) {
			UIModel.deployedItemSummonsUI[p_currentCount].gameObject.SetActive(true);
			UIModel.deployedItemSummonsUI[p_currentCount].MakeSlotLocked(p_currentMana >= UIModel.deployedItemSummonsUI[p_currentCount].unlockCost);
		}
		
		for (int x = p_currentCount + 1; x < UIModel.deployedItemSummonsUI.Count; ++x) {
			UIModel.deployedItemSummonsUI[x].gameObject.SetActive(true);
			UIModel.deployedItemSummonsUI[x].MakeSlotLockedNoButton();
		}
	}

	public void SetTitle(string p_title) {
		UIModel.txtTitle.text = p_title;
	}

	public void SetButtonDeployText(string p_text) {
		UIModel.btnDeploy.GetComponentInChildren<RuinarchText>().text = p_text;
	}

	public void HideAllSubMenu() {
		UIModel.subMinionContainer.SetActive(false);
		UIModel.subSummonContainer.SetActive(false);
		UIModel.subTargetContainer.SetActive(false);
	}
	#endregion
	public Transform GetAvailableTargetParent() {
		return UIModel.availableTargetsParent;
	}

	public Transform GetAvailableMinionsParent() {
		return UIModel.availableMinionsParent;
	}
	public Transform GetDeployedMinionsParent() {
		return UIModel.deployedMinionsParent;
	}
	public Transform GetDeployedSummonsParent() {
		return UIModel.deployedSummonsParent;
	}

	public void EnableDeployButton() {
		UIModel.btnDeploy.interactable = true;
	}

	public void DisableDeployButton() {
		UIModel.btnDeploy.interactable = false;
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onDeployClicked += p_listener.OnDeployClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onAddMinionClicked += p_listener.OnAddMinionClicked;
		UIModel.onAddTargetClicked += p_listener.OnAddTargetClicked;
		UIModel.onCloseSummonSubContainer += p_listener.OnCloseSummonSubContainer;
		UIModel.onCloseMinionSubContainer += p_listener.OnCloseMinionSubContainer;
		UIModel.onCloseTargetSubContainer += p_listener.OnCloseTargetSubContainer;
		UIModel.onHoverOver += p_listener.OnHoverOver;
		UIModel.onHoverOut += p_listener.OnHoverOut;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onDeployClicked -= p_listener.OnDeployClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onAddMinionClicked -= p_listener.OnAddMinionClicked;
		UIModel.onAddTargetClicked -= p_listener.OnAddTargetClicked;
		UIModel.onCloseSummonSubContainer -= p_listener.OnCloseSummonSubContainer;
		UIModel.onCloseMinionSubContainer -= p_listener.OnCloseMinionSubContainer;
		UIModel.onCloseTargetSubContainer -= p_listener.OnCloseTargetSubContainer;
		UIModel.onHoverOver -= p_listener.OnHoverOver;
		UIModel.onHoverOut -= p_listener.OnHoverOut;
	}
	#endregion
}