using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class TransmissionUIController : MVCUIController, TransmissionUIView.IListener
{
	[SerializeField]
	private TransmissionUIModel m_transmissionUIModel;
	private TransmissionUIView m_transmissionUIView;

	private float m_airbornePrice = 100f;
	private float m_consumptionPrice = 100f;
	private float m_directContactPrice = 100f;
	private float m_combatPrice = 100f;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		TransmissionUIView.Create(_canvas, m_transmissionUIModel, (p_ui) => {
			m_transmissionUIView = p_ui;
			m_transmissionUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	#region TransmissionUIView.IListener implementation
	public void OnAirBorneUpgradeClicked() { m_transmissionUIView.UpdateAirbornePrice(m_airbornePrice++.ToString()); }
	public void OnConsumptionUpgradeClicked() { m_transmissionUIView.UpdateConsumptionPrice(m_consumptionPrice++.ToString()); }
	public void OnDirectContactUpgradeClicked() { m_transmissionUIView.UpdateDirectContactPrice(m_directContactPrice++.ToString()); }
	public void OnCombatUpgradeClicked() { m_transmissionUIView.UpdateCombatPrice(m_combatPrice++.ToString()); }
	#endregion
}