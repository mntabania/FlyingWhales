using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class MaraudUIModel : MVCUIModel {

	public Action onAddMinionClicked;
	public Action onAddSummonClicked;
	public Action onAddTargetClicked;
	public Action onDeployClicked;
	public Action onCloseClicked;

	public List<DeployedMonsterItemUI> deployedItemSummonsUI = new List<DeployedMonsterItemUI>();
	public List<DeployedMonsterItemUI> deployedItemMinionsUI = new List<DeployedMonsterItemUI>();

	public Button btnDeploy;
	public Button btnClose;
	public Button btnAddSummon;
	public Button btnAddMinion;
	public Button btnAddTarget;

	public Transform scrollViewDeployedSummons;
	public Transform scrollViewDeployedMinions;
	public Transform scrollViewDeployedTargets;
	public Transform availableSummonsParent;
	public Transform availableMinionsParent;
	public Transform deployedMinionsParent;
	public Transform deployedSummonsParent;

	public GameObject subSummonContainer;
	public GameObject subMinionContainer;
	public GameObject subTargetContainer;

	public Text txtTitle;

	private void OnEnable() {
		btnDeploy.onClick.AddListener(ClickDeploy);
		btnClose.onClick.AddListener(ClickClose);
		btnAddSummon.onClick.AddListener(ClickAddSummon);
		btnAddMinion.onClick.AddListener(ClickAddMinion);
		btnAddTarget.onClick.AddListener(ClickAddTarget);
	}

	private void OnDisable() {
		btnDeploy.onClick.RemoveListener(ClickDeploy);
		btnClose.onClick.RemoveListener(ClickClose);
		btnAddSummon.onClick.RemoveListener(ClickAddSummon);
		btnAddMinion.onClick.RemoveListener(ClickAddMinion);
		btnAddTarget.onClick.RemoveListener(ClickAddTarget);
	}

	#region Buttons OnClick trigger
	void ClickDeploy() {
		onDeployClicked?.Invoke();
	}

	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickAddSummon() {
		onAddSummonClicked?.Invoke();
	}

	void ClickAddMinion() {
		onAddMinionClicked?.Invoke(); 
	}

	void ClickAddTarget() {
		onAddTargetClicked?.Invoke();
	}
	#endregion
}