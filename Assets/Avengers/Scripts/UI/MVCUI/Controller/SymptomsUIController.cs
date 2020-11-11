using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class SymptomsUIController : MVCUIController, SymptomsUIView.IListener
{
	[SerializeField]
	private SymptomsUIModel m_symptomsUIModel;
	private SymptomsUIView m_symptomsUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SymptomsUIView.Create(_canvas, m_symptomsUIModel, (p_ui) => {
			m_symptomsUIView = p_ui;
			m_symptomsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	#region SymptomsUIView.IListener implementation
	public void OnParalysisUpgradeClicked() { Debug.Log("OnParalysisUpgradeClicked"); }
	public void OnVomitingUpgradeClicked() { Debug.Log("OnVomitingUpgradeClicked"); }
	public void OnLethargyUpgradeClicked() { Debug.Log("OnLethargyUpgradeClicked"); }
	public void OnSeizuresUpgradeClicked() { Debug.Log("OnSeizuresUpgradeClicked"); }
	public void OnInsomniaUpgradeClicked() { Debug.Log("OnInsomniaUpgradeClicked"); }
	public void OnPoisonCloudUpgradeClicked() { Debug.Log("OnPoisonCloudUpgradeClicked"); }
	public void OnMonsterScenetUpgradeClicked() { Debug.Log("OnMonsterScenetUpgradeClicked"); }
	public void OnSneezingUpgradeClicked() { Debug.Log("OnSneezingUpgradeClicked"); }
	public void OnDepressionUpgradeClicked() { Debug.Log("OnDepressionUpgradeClicked"); }
	public void OnHungerPangsUpgradeClicked() { Debug.Log("OnHungerPangsUpgradeClicked"); }
	#endregion
}