﻿using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseSkillUIModel : MVCUIModel {

	public Action onCloseClicked;
	public Action onRerollClicked;

	public RuinarchButton btnClose;
	public RuinarchButton btnReroll;

	public RuinarchText txtMessageDisplay;
	public Transform skillsParent;

	private void OnEnable() {
		btnClose.onClick.AddListener(ClickClose);
		btnReroll.onClick.AddListener(ClickReroll);
	}

	private void OnDisable() {
		
		btnClose.onClick.RemoveListener(ClickClose);
		btnReroll.onClick.RemoveListener(ClickReroll);
	}

	#region Buttons OnClick trigger
	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickReroll() {
		onRerollClicked?.Invoke();
	}
	#endregion
}