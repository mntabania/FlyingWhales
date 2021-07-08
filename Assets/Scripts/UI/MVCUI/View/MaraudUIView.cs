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
		void OnClickNoTargetsTip();
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

	public void ShowNoTargetsUI(string p_text) {
		UIModel.txtEmptyTargetList.gameObject.SetActive(true);
		UIModel.txtEmptyTargetList.text = p_text;
		UIModel.btnNoTargetsTip.gameObject.SetActive(true);
	}
	public void HideNoTargetsUI() {
		UIModel.txtEmptyTargetList.gameObject.SetActive(false);
		UIModel.btnNoTargetsTip.gameObject.SetActive(false);
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

	public void ProcessMinionDisplay(int p_deployedMinionCount) {
		if (p_deployedMinionCount <= 0) {
			if (!UIModel.subMinionContainer.gameObject.activeSelf) {
				UIModel.btnAddMinion.gameObject.SetActive(true);
			} else {
				UIModel.btnAddMinion.gameObject.SetActive(false);
			}
		} else {
			UIModel.btnAddMinion.gameObject.SetActive(false);
		}
	}

	public void ProcessTargetDisplay(int p_deployedTargetCount) {
		if (p_deployedTargetCount <= 0) {
			if (!UIModel.subTargetContainer.gameObject.activeSelf) {
				UIModel.btnAddTarget.gameObject.SetActive(true);
			} else {
				UIModel.btnAddTarget.gameObject.SetActive(false);
			}
		} else {
			UIModel.btnAddTarget.gameObject.SetActive(false);
		}
	}

	public void ProcessSummonDisplay(int p_currentCount, int p_maxCount, Party p_party, int p_currentMana) {
		int targetIndex = -1;
		for (int x = 0; x < p_currentCount; ++x) {
			UIModel.deployedItemSummonsUI[x].gameObject.SetActive(true);
		}
		for (int x = 0; x < p_currentCount; ++x) {
			if (!UIModel.deployedItemSummonsUI[x].isDeployed && !UIModel.deployedItemSummonsUI[x].isReadyForDeploy) {
				UIModel.deployedItemSummonsUI[x].MakeSlotEmpty();
				if (targetIndex == -1) {
					targetIndex = x;
				}
			}
		}
		if (targetIndex == -1) {
			targetIndex = p_currentCount;
		}
		if (targetIndex < p_currentCount) {
			UIModel.deployedItemSummonsUI[targetIndex].gameObject.SetActive(true);
			if (!UIModel.subSummonContainer.gameObject.activeSelf) {
				UIModel.deployedItemSummonsUI[targetIndex].DisplayAddSummon();
			} else {
				UIModel.deployedItemSummonsUI[targetIndex].MakeSlotEmpty();
			}
		}
		targetIndex = p_currentCount;
		if (targetIndex < p_maxCount) {
			UIModel.deployedItemSummonsUI[targetIndex].gameObject.SetActive(true);
			UIModel.deployedItemSummonsUI[targetIndex].MakeSlotLocked(p_currentMana >= UIModel.deployedItemSummonsUI[targetIndex].GetUnlockCost());
			targetIndex++;
			for (int x = targetIndex; x < p_maxCount; ++x) {
				UIModel.deployedItemSummonsUI[x].gameObject.SetActive(true);
				UIModel.deployedItemSummonsUI[x].MakeSlotLockedNoButton();
			}
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

	public void DisplayToolTipWithCharge(string p_name, string p_description, string p_chargeDisplay) {
		
		UIModel.monsterToolTipUI.DisplayToolTipWithCharge(p_name, p_description, p_chargeDisplay);
	}

	public void DisplayToolTipWithoutCharge(string p_name, string p_description) {
		UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(p_name, p_description);
	}

	public void HideToolTip() {
		UIModel.monsterToolTipUI.HideToolTip();
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
		UIModel.onNoTargetsTipClicked += p_listener.OnClickNoTargetsTip;
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
		UIModel.onNoTargetsTipClicked -= p_listener.OnClickNoTargetsTip;
	}
	#endregion
}