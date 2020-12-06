using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class RightClickUIController : MVCUIController
{
	[SerializeField]
	private RightClickUIModel m_rightClickUIModel;
	private RightClickUIView m_rightClickUIView;

	private void OnEnable()
	{
		ClickableMenuUIObject.onMenuPress += OnMenuClicked;
	}

	private void OnDisable()
	{
		ClickableMenuUIObject.onMenuPress -= OnMenuClicked;
	}

	private void Start()
	{
		InstantiateUI();
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		RightClickUIView.Create(_canvas, m_rightClickUIModel, (p_ui) => {
			m_rightClickUIView = p_ui;
			InitUI(p_ui.UIModel, p_ui);
			m_rightClickUIView.InitializedMenu();
		});
	}

	void OnMenuClicked(ParentUIData p_UIMenu, bool p_isAction, int p_currentColumn) 
	{
		if (!p_isAction)
		{
			m_rightClickUIView.DisplaySubMenu(p_UIMenu.subMenu, p_currentColumn + 1);
		} else 
		{
			Debug.Log(p_UIMenu.clickableMenuData.strMenuName + " ACTION TRIGGERED");
		}
	}
}