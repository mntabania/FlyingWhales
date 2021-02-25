using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class DefensePointUIModel : MVCUIModel {
	public Action onDeployClicked;
	public Action onCloseClicked;
	
	public Button btnDeploy;
	public Button btnClose;

	public Transform availableMonstersParent;
	public Transform deplyedMonstersParent;

	private void OnEnable() {
		btnDeploy.onClick.AddListener(ClickDeploy);
		btnClose.onClick.AddListener(ClickClose);
	}

	private void OnDisable() {
		btnDeploy.onClick.RemoveListener(ClickDeploy);
		btnClose.onClick.RemoveListener(ClickClose);
	}

	#region Buttons OnClick trigger
	void ClickDeploy() {
		onDeployClicked?.Invoke();
	}

	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	#endregion
}