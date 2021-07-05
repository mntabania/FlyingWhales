using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class DefensePointUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnDeployClicked();
		void OnCloseClicked();
		
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
			if (!UIModel.subSummonContainer.activeSelf) {
				UIModel.deployedItemSummonsUI[lastAvailIndex].DisplayAddSummon();
			} else {
				UIModel.deployedItemSummonsUI[lastAvailIndex].MakeSlotEmpty();
			}
		}
		if (p_currentCount < p_maxCount) {
			UIModel.deployedItemSummonsUI[p_currentCount].gameObject.SetActive(true);
			UIModel.deployedItemSummonsUI[p_currentCount].MakeSlotLocked(p_currentMana >= UIModel.deployedItemSummonsUI[p_currentCount].GetUnlockCost());
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
		UIModel.onCloseSummonSubContainer += p_listener.OnCloseSummonSubContainer;
		UIModel.onHoverOver += p_listener.OnHoverOver;
		UIModel.onHoverOut += p_listener.OnHoverOut;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onDeployClicked -= p_listener.OnDeployClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onCloseSummonSubContainer -= p_listener.OnCloseSummonSubContainer;
		UIModel.onHoverOver -= p_listener.OnHoverOver;
		UIModel.onHoverOut -= p_listener.OnHoverOut;
	}
	#endregion
}