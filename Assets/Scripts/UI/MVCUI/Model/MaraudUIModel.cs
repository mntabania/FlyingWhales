using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class MaraudUIModel : MVCUIModel {

	public Action<bool> onLesserDemonClicked;
	public Action<bool> onMinionsClicked;
	public Action onDeployClicked;
	public Action onCloseClicked;

	public Button btnDeploy;
	public Button btnClose;
	public RuinarchToggle btnLesserDemonsTab;
	public RuinarchToggle btnMinionsTab;

	public Transform scrollViewLesserDemon;
	public Transform scrollViewMinions;
	public Transform availableLesserDemonParent;
	public Transform availableMinionsParent;
	public Transform deplyedMonstersParent;

	private void OnEnable() {
		btnDeploy.onClick.AddListener(ClickDeploy);
		btnClose.onClick.AddListener(ClickClose);
		btnLesserDemonsTab.onValueChanged.AddListener(ClickLesserDemonsTab);
		btnMinionsTab.onValueChanged.AddListener(ClickMinionsTab);
	}

	private void OnDisable() {
		btnDeploy.onClick.RemoveListener(ClickDeploy);
		btnClose.onClick.RemoveListener(ClickClose);
		btnLesserDemonsTab.onValueChanged.RemoveListener(ClickLesserDemonsTab);
		btnMinionsTab.onValueChanged.RemoveListener(ClickMinionsTab);
	}

	#region Buttons OnClick trigger
	void ClickDeploy() {
		onDeployClicked?.Invoke();
	}

	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickLesserDemonsTab(bool isOn) {
		onLesserDemonClicked?.Invoke(isOn);
	}

	void ClickMinionsTab(bool isOn) {
		onMinionsClicked?.Invoke(isOn);
	}
	#endregion
}