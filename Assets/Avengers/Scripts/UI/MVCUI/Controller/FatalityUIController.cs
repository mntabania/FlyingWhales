using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class FatalityUIController : MVCUIController, FatalityUIView.IListener
{
	[SerializeField]
	private FatalityUIModel m_fatalityUIModel;
	private FatalityUIView m_fatalityUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		FatalityUIView.Create(_canvas, m_fatalityUIModel, (p_ui) => {
			m_fatalityUIView = p_ui;
			m_fatalityUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	#region FatalityUIView.IListener implementation
	public void OnSepticShockUpgradeClicked() { Debug.Log("OnSepticShockUpgradeClicked"); }
	public void OnHeartAttackUpgradeClicked() { Debug.Log("OnHeartAttackUpgradeClicked"); }
	public void OnStrokeUpgradeClicked() { Debug.Log("OnStrokeUpgradeClicked"); }
	public void OnTotalOrganFailureUpgradeClicked() { Debug.Log("OnTotalOrganFailureUpgradeClicked"); }
	public void OnPneumoniaUpgradeClicked() { Debug.Log("OnPneumoniaUpgradeClicked"); }
	#endregion
}