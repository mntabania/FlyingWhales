using Ruinarch.MVCFramework;
using System;
using UnityEngine;
using UnityEngine.UI;
public class TransmissionUIModel : MVCUIModel
{
	public Action onAirBorneUpgradeClicked;
	public Action onConsumptionUpgradeClicked;
	public Action onDirectContactUpgradeClicked;
	public Action onCombatUpgradeClicked;
	
	public Button btnAirBorneUpgrade;
	public Button btnConsumptionUpgrade;
	public Button btnDirectContactUpgrade;
	public Button btnCombatUpgrade;

	public RuinarchText txtAirBorneCost;
	public RuinarchText txtAirBorneRate;
	public RuinarchText txtConsumptionCost;
	public RuinarchText txtConsumptionRate;
	public RuinarchText txtDirectContactCost;
	public RuinarchText txtDirectContactRate;
	public RuinarchText txtCombatUpgradeCost;
	public RuinarchText txtCombatRate;

	private void OnEnable()
	{
		btnAirBorneUpgrade.onClick.AddListener(ClickAirBorneUpgrade);
		btnConsumptionUpgrade.onClick.AddListener(ClickConsumptionUpgrade);
		btnDirectContactUpgrade.onClick.AddListener(ClickDirectContactUpgrade);
		btnCombatUpgrade.onClick.AddListener(ClickCombatUpgrade);
	}

	private void OnDisable()
	{
		btnAirBorneUpgrade.onClick.RemoveListener(ClickAirBorneUpgrade);
		btnConsumptionUpgrade.onClick.RemoveListener(ClickConsumptionUpgrade);
		btnDirectContactUpgrade.onClick.RemoveListener(ClickDirectContactUpgrade);
		btnCombatUpgrade.onClick.RemoveListener(ClickCombatUpgrade);
	}

	#region Buttons OnClick trigger
	void ClickAirBorneUpgrade()
	{
		onAirBorneUpgradeClicked?.Invoke();
	}

	void ClickConsumptionUpgrade()
	{
		onConsumptionUpgradeClicked?.Invoke();
	}
	void ClickDirectContactUpgrade()
	{
		onDirectContactUpgradeClicked?.Invoke();
	}

	void ClickCombatUpgrade()
	{
		onCombatUpgradeClicked?.Invoke();
	}
	#endregion
}