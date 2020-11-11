using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class OnDeathUIController : MVCUIController, OnDeathUIView.IListener
{
	[SerializeField]
	private OnDeathUIModel m_onDeathUIModel;
	private OnDeathUIView m_onDeathUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		OnDeathUIView.Create(_canvas, m_onDeathUIModel, (p_ui) => {
			m_onDeathUIView = p_ui;
			m_onDeathUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	#region OnDeathUIView.IListener implementation
	public void OnIgniteUpgradeClicked() { Debug.Log("OnIgniteUpgradeClicked"); }
	public void OnWalkerZombieUpgradeClicked() { Debug.Log("OnWalkerZombieUpgradeClicked"); }
	public void OnMana2_3UpgradeClicked() { Debug.Log("OnMana2_3UpgradeClicked"); }
	public void OnRandomSpirit_1UpgradeClicked() { Debug.Log("OnRandomSpirit_1UpgradeClicked"); }
	#endregion
}