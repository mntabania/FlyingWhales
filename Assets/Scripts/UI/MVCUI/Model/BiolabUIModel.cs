using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class BiolabUIModel : MVCUIModel
{
	public Action<bool> onTransmissionTabClicked;
	public Action<bool> onLifeSpanTabClicked;
	public Action<bool> onFatalityTabClicked;
	public Action<bool> onSymptomsTabClicked;
	public Action<bool> onOnDeathClicked;
	public Action onCloseClicked;
	
	public RuinarchToggle btnTransmissionTab;
	public RuinarchToggle btnLifeSpanTab;
	public RuinarchToggle btnFatalityTab;
	public RuinarchToggle btnSymptomsTab;
	public RuinarchToggle btnOnDeathTab;
	public Button btnClose;

	public RuinarchText txtActiveCasesValue;
	public RuinarchText txtDeathsValue;
	public RuinarchText txtRecoveriesValue;
	public RuinarchText txtPlagueRatsValue;
	public RuinarchText txtPlaguePoints;

	public Transform tabPrent;

	private void OnEnable()
	{
		btnTransmissionTab.onValueChanged.AddListener(ClickTransmissionTab);
		btnLifeSpanTab.onValueChanged.AddListener(ClickLifeSpanTab);
		btnFatalityTab.onValueChanged.AddListener(ClickFatalityTab);
		btnSymptomsTab.onValueChanged.AddListener(ClickSymptomsTab);
		btnOnDeathTab.onValueChanged.AddListener(ClickOnDeathTab);
		btnClose.onClick.AddListener(ClickClose);
	}

	private void OnDisable()
	{
		btnTransmissionTab.onValueChanged.RemoveListener(ClickTransmissionTab);
		btnLifeSpanTab.onValueChanged.RemoveListener(ClickLifeSpanTab);
		btnFatalityTab.onValueChanged.RemoveListener(ClickFatalityTab);
		btnSymptomsTab.onValueChanged.RemoveListener(ClickSymptomsTab);
		btnOnDeathTab.onValueChanged.RemoveListener(ClickOnDeathTab);
		btnClose.onClick.RemoveListener(ClickClose);
	}

	#region Buttons OnClick trigger
	void ClickTransmissionTab(bool isOn)
	{
		onTransmissionTabClicked?.Invoke(isOn);
	}

	void ClickLifeSpanTab(bool isOn)
	{
		onLifeSpanTabClicked?.Invoke(isOn);
	}
	void ClickFatalityTab(bool isOn)
	{
		onFatalityTabClicked?.Invoke(isOn);
	}

	void ClickSymptomsTab(bool isOn)
	{
		onSymptomsTabClicked?.Invoke(isOn);
	}

	void ClickOnDeathTab(bool isOn)
	{
		onOnDeathClicked?.Invoke(isOn);
	}
	void ClickClose()
	{
		onCloseClicked?.Invoke();
	}	
	#endregion
}