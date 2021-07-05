using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilityScripts;

public class ContextMenuUIView : MVCUIView
{
	List<ContextMenuUIObject> clickableMenuUIObjects = new List<ContextMenuUIObject>();
	
	#region interface for listener
	public interface IListener {
		void OnHoverOverParentDisplay();
		void OnHoverOutParentDisplay();
	}
	#endregion
	
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public ContextMenuUIModel UIModel
	{
		get
		{
			return _baseAssetModel as ContextMenuUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, ContextMenuUIModel p_assets, Action<ContextMenuUIView> p_onCreate)
	{
		var go = new GameObject(typeof(ContextMenuUIView).ToString());
		var gui = go.AddComponent<ContextMenuUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	public void InitializeUI(List<IContextMenuItem> p_mainItems, Canvas p_canvas) {
		DisplayMenu(p_mainItems, 0, p_canvas);
	}
	
	private void DisplayMenu(List<IContextMenuItem> p_UIMenu, int p_targetColumn, Canvas p_canvas, bool dontShowName = false) {
		clickableMenuUIObjects.Clear();
		ScrollRect columnScrollRect = UIModel.menuParent[p_targetColumn].scrollRect;
		columnScrollRect.gameObject.SetActive(true);

		clickableMenuUIObjects.AddRange(columnScrollRect.content.GetComponentsInChildren<ContextMenuUIObject>());
		int count = clickableMenuUIObjects.Count;
		for (int x = 0; x < count; ++x) {
			ObjectPoolManager.Instance.DestroyObject(clickableMenuUIObjects[x]);
		}
		if (p_UIMenu != null) {
			for (int x = 0; x < p_UIMenu.Count; ++x) {
				GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("ContextMenuItem", Vector3.zero, Quaternion.identity, columnScrollRect.content.transform);
				ContextMenuUIObject contextMenuUI = go.GetComponent<ContextMenuUIObject>();
				contextMenuUI.SetMenuDetails(p_UIMenu[x], dontShowName);
				contextMenuUI.btnActivate.ForceUpdateGlow();
			}	
		}
	}
	public void HideColumn(int p_targetColumn) {
		if (p_targetColumn >= 0 && p_targetColumn < UIModel.menuParent.Length) {
			ScrollRect columnRect = UIModel.menuParent[p_targetColumn].scrollRect;
			columnRect.gameObject.SetActive(false);
		}
	}
	public void DisplaySubMenu(List<IContextMenuItem> p_UIMenu, int p_targetColumn, Canvas p_canvas, bool dontShowName = false) {
		DisplayMenu(p_UIMenu, p_targetColumn, p_canvas, dontShowName);
		ScrollRect columnScrollRect = UIModel.menuParent[p_targetColumn].scrollRect;
		columnScrollRect.gameObject.SetActive(true);
		if (p_targetColumn == 1) {
			RectTransform columnRect = columnScrollRect.transform as RectTransform;
			columnRect.anchoredPosition = UIModel.column2RightPos;
			if (!GameUtilities.IsRectFullyInCanvas(columnRect, p_canvas.transform as RectTransform)) {
				columnRect.anchoredPosition = UIModel.column2LeftPos;
			}
		}
	}
	public void SetTitleName(string p_Name) {
		UIModel.lblTitle.text = p_Name;
	}
	public void SetPosition(Vector3 p_pos, Canvas p_canvas) {
		UIModel.parentDisplay.position = p_pos;
	}
	private ContextMenuColumn GetLeftMostColumn() {
		if (UIModel.menuParent[1].gameObject.activeSelf && UIModel.menuParent[1].rectTransform.anchoredPosition == UIModel.column2LeftPos) {
			//2nd column is in left position
			return UIModel.menuParent[1];
		} else {
			return UIModel.menuParent[0];	
		}
	}
	private ContextMenuColumn GetRightMostColumn() {
		if (UIModel.menuParent[1].gameObject.activeSelf && UIModel.menuParent[1].rectTransform.anchoredPosition == UIModel.column2RightPos) {
			//2nd column is in right position
			return UIModel.menuParent[1];
		} else {
			return UIModel.menuParent[0];	
		}
	}
	public UIHoverPosition GetTooltipHoverPositionToUse() {
		ContextMenuColumn leftMostColumn = GetLeftMostColumn();
		RectTransform leftColumnLeftHoverPosRect = leftMostColumn.leftHoverPosition.transform as RectTransform;
		Rect screenRect = new Rect (0,0, Screen.width, Screen.height);
		if (GameUtilities.IsRectFullyInCanvas(leftColumnLeftHoverPosRect, screenRect)) {
			return leftMostColumn.leftHoverPosition;
		} else {
			ContextMenuColumn rightMostColumn = GetRightMostColumn();
			return rightMostColumn.rightHoverPosition;
		}
	}

	
	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.parentDisplayHoverOver += p_listener.OnHoverOverParentDisplay;
		UIModel.parentDisplayHoverOut += p_listener.OnHoverOutParentDisplay;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.parentDisplayHoverOver -= p_listener.OnHoverOverParentDisplay;
		UIModel.parentDisplayHoverOut -= p_listener.OnHoverOutParentDisplay;
	}
	#endregion
}