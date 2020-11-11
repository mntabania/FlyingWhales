using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class BiolabUIController : MVCUIController, BiolabUIView.IListener
{
	[SerializeField]
	private BiolabUIModel m_biolabUIModel;
	private BiolabUIView m_biolabUIView;

	public TransmissionUIController transmissionUIController;
	public LifeSpanUIController lifeSpanUIController;
	public FatalityUIController fatalityUIController;
	public SymptomsUIController symptomsUIController;
	public OnDeathUIController onDeathUIController;

	private void Start()
	{
		InstantiateUI();
	}

	private void OnDisable()
	{
		onDeathUIController.onUIINstantiated -= LastUINstantiate;
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		BiolabUIView.Create(_canvas, m_biolabUIModel, (p_ui) => {
			m_biolabUIView = p_ui;
			m_biolabUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);

			onDeathUIController.onUIINstantiated += LastUINstantiate;

			m_biolabUIView.SetActivaCases("12345");
			m_biolabUIView.SetDeathCases("10");
			m_biolabUIView.SetRecoveriesCases("0");
			m_biolabUIView.SetPlagueRats("10/10");
			m_biolabUIView.SetPlaguePoints("5000");

			transmissionUIController.InstantiateUI();
			lifeSpanUIController.InstantiateUI();
			fatalityUIController.InstantiateUI();
			symptomsUIController.InstantiateUI();
			onDeathUIController.InstantiateUI();
		});
	}

	void LastUINstantiate() 
	{
		transmissionUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		lifeSpanUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		fatalityUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		symptomsUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		onDeathUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		ShowUI(transmissionUIController);
	}

	void ShowUI(MVCUIController p_targetUIToShow) {
		transmissionUIController.HideUI();
		lifeSpanUIController.HideUI();
		fatalityUIController.HideUI();
		symptomsUIController.HideUI();
		onDeathUIController.HideUI();

		p_targetUIToShow.ShowUI();
	}

	#region BiolabUIView.IListener implementation
	public void OnTransmissionTabClicked() { ShowUI(transmissionUIController); }
	public void OnLifeSpanTabClicked() { ShowUI(lifeSpanUIController); }
	public void OnFatalityTabClicked() { ShowUI(fatalityUIController); }
	public void OnSymptomsTabClicked() { ShowUI(symptomsUIController); }
	public void OnOnDeathClicked() { ShowUI(onDeathUIController); }
	public void OnCloseClicked() { Debug.Log("OnCloseClicked"); }
	#endregion
}