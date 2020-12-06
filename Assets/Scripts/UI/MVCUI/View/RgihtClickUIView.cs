using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Collections.Generic;
using System.Linq;

public class RightClickUIView : MVCUIView
{
	List<ClickableMenuUIObject> clickableMenuUIObjects = new List<ClickableMenuUIObject>();
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public RightClickUIModel UIModel
	{
		get
		{
			return _baseAssetModel as RightClickUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, RightClickUIModel p_assets, Action<RightClickUIView> p_onCreate)
	{
		var go = new GameObject(typeof(RightClickUIView).ToString());
		var gui = go.AddComponent<RightClickUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	public void InitializedMenu() 
	{
		DisplayMenu(UIModel.parentUIMenu.subMenu, 0);
	}

	void DisplayMenu(List<ParentUIData> p_UIMenu, int p_targetColumn) 
	{
		clickableMenuUIObjects.Clear();
		clickableMenuUIObjects = UIModel.menuParent[p_targetColumn].GetComponentsInChildren<ClickableMenuUIObject>().ToList();
		int count = clickableMenuUIObjects.Count;
		for (int x = 0; x < count; ++x) 
		{
			DestroyImmediate(clickableMenuUIObjects[x].gameObject);
		}
		for (int x = 0; x < p_UIMenu.Count; ++x)
		{
			GameObject go = Instantiate(Resources.Load("UI/ClickableButton")) as GameObject;
			ClickableMenuUIObject clickableMenuUI = go.GetComponent<ClickableMenuUIObject>();
			clickableMenuUI.SetMenuDetails(p_UIMenu[x]);
			clickableMenuUI.transform.SetParent(UIModel.menuParent[p_targetColumn].transform);
			clickableMenuUIObjects.Add(clickableMenuUI);
		}
	}

	public void DisplaySubMenu(List<ParentUIData> p_UIMenu, int p_targetColumn) 
	{
		DisplayMenu(p_UIMenu, p_targetColumn);
	}
}