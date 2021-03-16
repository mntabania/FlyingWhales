using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseSkillUIModel : MVCUIModel {

	public Action onCloseClicked;
	public Action onRerollClicked;
	public Action onHoverOverReroll;
	public Action onHoverOutReroll;

	public RuinarchButton btnClose;
	public RuinarchButton btnReroll;
	public HoverHandler hoverHandlerReroll;
	
	public RuinarchText txtMessageDisplay;
	public Transform skillsParent;

	private void OnEnable() {
		btnClose.onClick.AddListener(ClickClose);
		btnReroll.onClick.AddListener(ClickReroll);
		hoverHandlerReroll.AddOnHoverOverAction(OnHoverOverReroll);
		hoverHandlerReroll.AddOnHoverOutAction(OnHoverOutReroll);
	}

	private void OnDisable() {
		btnClose.onClick.RemoveListener(ClickClose);
		btnReroll.onClick.RemoveListener(ClickReroll);
		hoverHandlerReroll.RemoveOnHoverOverAction(OnHoverOverReroll);
		hoverHandlerReroll.RemoveOnHoverOutAction(OnHoverOutReroll);
	}

	#region Buttons OnClick trigger
	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickReroll() {
		onRerollClicked?.Invoke();
	}
	void OnHoverOverReroll() {
		onHoverOverReroll?.Invoke();	
	}
	void OnHoverOutReroll() {
		onHoverOutReroll?.Invoke();	
	}
	#endregion
}