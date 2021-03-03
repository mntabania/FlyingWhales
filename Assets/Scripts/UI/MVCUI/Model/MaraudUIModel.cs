using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class MaraudUIModel : MVCUIModel {

	public Action<bool> onMinionClicked;
	public Action<bool> onSummonClicked;
	public Action onDeployClicked;
	public Action onCloseClicked;

	public List<DeployedMonsterItemUI> deployedItemSummonsUI = new List<DeployedMonsterItemUI>();
	public List<DeployedMonsterItemUI> deployedItemMinionsUI = new List<DeployedMonsterItemUI>();

	public Button btnDeploy;
	public Button btnClose;
	public RuinarchToggle btnSummonsTab;
	public RuinarchToggle btnMinionsTab;

	public Transform scrollViewSummons;
	public Transform scrollViewMinions;
	public Transform availableSummonsParent;
	public Transform availableMinionsParent;
	public Transform deployedMinionsParent;
	public Transform deployedSummonsParent;

	private void OnEnable() {
		btnDeploy.onClick.AddListener(ClickDeploy);
		btnClose.onClick.AddListener(ClickClose);
		btnSummonsTab.onValueChanged.AddListener(ClickSummonsTab);
		btnMinionsTab.onValueChanged.AddListener(ClickMinionsTab);
	}

	private void OnDisable() {
		btnDeploy.onClick.RemoveListener(ClickDeploy);
		btnClose.onClick.RemoveListener(ClickClose);
		btnSummonsTab.onValueChanged.RemoveListener(ClickSummonsTab);
		btnMinionsTab.onValueChanged.RemoveListener(ClickMinionsTab);
	}

	#region Buttons OnClick trigger
	void ClickDeploy() {
		onDeployClicked?.Invoke();
	}

	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickSummonsTab(bool isOn) {
		onSummonClicked?.Invoke(isOn);
	}

	void ClickMinionsTab(bool isOn) {
		onMinionClicked?.Invoke(isOn); 
	}
	#endregion
}