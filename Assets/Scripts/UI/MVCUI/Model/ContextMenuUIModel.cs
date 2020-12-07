using Ruinarch.MVCFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ContextMenuUIModel : MVCUIModel {
	public TextMeshProUGUI lblTitle;
	
	public ScrollRect[] menuParent;
	
	public Vector3 column2RightPos;
	public Vector3 column2LeftPos;

	public HoverHandler hoverHandlerParentDisplay;

	public System.Action parentDisplayHoverOver;
	public System.Action parentDisplayHoverOut;

	private void OnEnable() {
		hoverHandlerParentDisplay.AddOnHoverOverAction(OnHoverOverParentDisplay);
		hoverHandlerParentDisplay.AddOnHoverOutAction(OnHoverOutParentDisplay);
	}

	private void OnDisable() {
		hoverHandlerParentDisplay.RemoveOnHoverOverAction(OnHoverOverParentDisplay);
		hoverHandlerParentDisplay.RemoveOnHoverOutAction(OnHoverOutParentDisplay);
	}

	private void OnHoverOverParentDisplay() {
		parentDisplayHoverOver?.Invoke();
	}
	private void OnHoverOutParentDisplay() {
		parentDisplayHoverOut?.Invoke();
	}
}