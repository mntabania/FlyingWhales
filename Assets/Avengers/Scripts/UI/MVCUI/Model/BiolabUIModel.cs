using Ruinarch.MVCFramework;
using System;
using UnityEngine;
using UnityEngine.UI;
public class BiolabUIModel : MVCUIModel
{
	public Action onTransmissionTabClicked;
	public Action onLifeSpanTabClicked;
	public Action onFatalityTabClicked;
	public Action onSymptomsTabClicked;
	public Action onOnDeathClicked;
	public Action onCloseClicked;
	
	public Button btnTransmissionTab;
	public Button btnLifeSpanTab;
	public Button btnFatalityTab;
	public Button btnSymptomsTab;
	public Button btnOnDeathTab;
	public Button btnClose;

	public RuinarchText txtActiveCasesValue;
	public RuinarchText txtDeathsValue;
	public RuinarchText txtRecoveriesValue;
	public RuinarchText txtPlagueRatsValue;
	public RuinarchText txtPlaguePoints;

	public Transform tabPrent;

	private void OnEnable()
	{
		btnTransmissionTab.onClick.AddListener(ClickTransmissionTab);
		btnLifeSpanTab.onClick.AddListener(ClickLifeSpanTab);
		btnFatalityTab.onClick.AddListener(ClickFatalityTab);
		btnSymptomsTab.onClick.AddListener(ClickSymptomsTab);
		btnOnDeathTab.onClick.AddListener(ClickOnDeath);
		btnClose.onClick.AddListener(ClickClose);
	}

	private void OnDisable()
	{
		btnTransmissionTab.onClick.RemoveListener(ClickTransmissionTab);
		btnLifeSpanTab.onClick.RemoveListener(ClickLifeSpanTab);
		btnFatalityTab.onClick.RemoveListener(ClickFatalityTab);
		btnSymptomsTab.onClick.RemoveListener(ClickSymptomsTab);
		btnOnDeathTab.onClick.RemoveListener(ClickOnDeath);
		btnClose.onClick.RemoveListener(ClickClose);
	}

	#region Buttons OnClick trigger
	void ClickTransmissionTab()
	{
		onTransmissionTabClicked?.Invoke();
	}

	void ClickLifeSpanTab()
	{
		onLifeSpanTabClicked?.Invoke();
	}
	void ClickFatalityTab()
	{
		onFatalityTabClicked?.Invoke();
	}

	void ClickSymptomsTab()
	{
		onSymptomsTabClicked?.Invoke();
	}

	void ClickOnDeath()
	{
		onOnDeathClicked?.Invoke();
	}
	void ClickClose()
	{
		onCloseClicked?.Invoke();
	}	
	#endregion
}