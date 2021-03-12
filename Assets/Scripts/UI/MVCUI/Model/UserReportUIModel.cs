using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class UserReportUIModel : MVCUIModel {

	public Action onCloseClicked;
	public Action onSubmitClicked;

	public RuinarchButton btnClose;
	public RuinarchButton btnSubmit;

	public InputField inpFldEmail;
	public InputField inpFldDescription;

	public UserReportingScript userReportingScript;

	private void OnEnable() {
		btnClose.onClick.AddListener(ClickClose);
		btnSubmit.onClick.AddListener(ClickReroll);
	}

	private void OnDisable() {
		btnClose.onClick.RemoveListener(ClickClose);
		btnSubmit.onClick.RemoveListener(ClickReroll);
	}

	#region Buttons OnClick trigger
	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickReroll() {
		onSubmitClicked?.Invoke();
	}
	#endregion
}