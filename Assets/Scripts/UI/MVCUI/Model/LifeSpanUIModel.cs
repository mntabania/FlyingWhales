using Ruinarch.MVCFramework;
using System;
using UnityEngine;
using UnityEngine.UI;
public class LifeSpanUIModel : MVCUIModel
{
	public Action onObjectsUpgradeClicked;
	public Action onElvesUpgradeClicked;
	public Action onHumansUpgradeClicked;
	public Action onMonstersUpgradeClicked;
	public Action onUndeadUpgradeClicked;

	public Button btnObjectsUpgrade;
	public Button btnElvesUpgrade;
	public Button btnHumansUpgrade;
	public Button btnMonstersUpgrade;
	public Button btnUndeadUpgrade;

	public RuinarchText txtTileObjectCost;
	public RuinarchText txtTileObjectInfectionTime;
	public RuinarchText txtElvesCost;
	public RuinarchText txtElvesInfectionTime;
	public RuinarchText txtHumansCost;
	public RuinarchText txtHumansInfectionTime;
	public RuinarchText txtMonstersCost;
	public RuinarchText txtMonstersInfectionTime;
	public RuinarchText txtUndeadCost;
	public RuinarchText txtUndeadInfectionTime;
	
	private void OnEnable()
	{
		btnObjectsUpgrade.onClick.AddListener(ClickObjectsUpgrade);
		btnElvesUpgrade.onClick.AddListener(ClickElvesUpgrade);
		btnHumansUpgrade.onClick.AddListener(ClickHumansUpgrade);
		btnMonstersUpgrade.onClick.AddListener(ClickMonstersUpgrade);
		btnUndeadUpgrade.onClick.AddListener(ClickUndeadUpgrade);
	}

	private void OnDisable()
	{
		btnObjectsUpgrade.onClick.RemoveListener(ClickObjectsUpgrade);
		btnElvesUpgrade.onClick.RemoveListener(ClickElvesUpgrade);
		btnHumansUpgrade.onClick.RemoveListener(ClickHumansUpgrade);
		btnMonstersUpgrade.onClick.RemoveListener(ClickMonstersUpgrade);
		btnUndeadUpgrade.onClick.RemoveListener(ClickUndeadUpgrade);
	}

	#region Buttons OnClick trigger
	void ClickObjectsUpgrade()
	{
		onObjectsUpgradeClicked?.Invoke();
	}

	void ClickElvesUpgrade()
	{
		onElvesUpgradeClicked?.Invoke();
	}
	void ClickHumansUpgrade()
	{
		onHumansUpgradeClicked?.Invoke();
	}

	void ClickMonstersUpgrade()
	{
		onMonstersUpgradeClicked?.Invoke();
	}

	void ClickUndeadUpgrade()
	{
		onUndeadUpgradeClicked?.Invoke();
	}
	#endregion
}