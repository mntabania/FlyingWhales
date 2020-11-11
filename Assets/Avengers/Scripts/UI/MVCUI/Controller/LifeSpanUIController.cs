using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class LifeSpanUIController : MVCUIController, LifeSpanUIView.IListener
{
	[SerializeField]
	private LifeSpanUIModel m_lifeSpanUIModel;
	private LifeSpanUIView m_lifeSpanUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		LifeSpanUIView.Create(_canvas, m_lifeSpanUIModel, (p_ui) => {
			m_lifeSpanUIView = p_ui;
			m_lifeSpanUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	#region LifeSpanUIView.IListener implementation
	public void OnObjectsUpgradeClicked() { Debug.Log("OnObjectsUpgradeClicked"); }
	public void OnElvesUpgradeClicked() { Debug.Log("OnElvesUpgradeClicked"); }
	public void OnHumansUpgradeClicked() { Debug.Log("OnHumansUpgradeClicked"); }
	public void OnMonstersUpgradeClicked() { Debug.Log("OnMonstersUpgradeClicked"); }
	public void OnUndeadUpgradeClicked() { Debug.Log("OnUndeadUpgradeClicked"); }
	#endregion
}