using Ruinarch.MVCFramework;
using System;
using UnityEngine;
using UnityEngine.UI;
public class OnDeathUIModel : MVCUIModel
{
	public Action onIgniteUpgradeClicked;
	public Action onWalkerZombieUpgradeClicked;
	public Action onMana2_3UpgradeClicked;
	public Action onRandomSpirit_1UpgradeClicked;

	public Button btnIgniteUpgrade;
	public Button btnWalkerZombieUpgrade;
	public Button btnMana2_3Upgrade;
	public Button btnRandomSpirit_1Upgrade;

	private void OnEnable()
	{
		btnIgniteUpgrade.onClick.AddListener(ClickIgniteUpgrade);
		btnWalkerZombieUpgrade.onClick.AddListener(ClickWalkerZombieUpgrade);
		btnMana2_3Upgrade.onClick.AddListener(ClickMana2_3Upgrade);
		btnRandomSpirit_1Upgrade.onClick.AddListener(ClickRandomSpirit_1Upgrade);
	}

	private void OnDisable()
	{
		btnIgniteUpgrade.onClick.RemoveListener(ClickIgniteUpgrade);
		btnWalkerZombieUpgrade.onClick.RemoveListener(ClickWalkerZombieUpgrade);
		btnMana2_3Upgrade.onClick.RemoveListener(ClickMana2_3Upgrade);
		btnRandomSpirit_1Upgrade.onClick.RemoveListener(ClickRandomSpirit_1Upgrade);
	}

	#region Buttons OnClick trigger
	void ClickIgniteUpgrade()
	{
		onIgniteUpgradeClicked?.Invoke();
	}

	void ClickWalkerZombieUpgrade()
	{
		onWalkerZombieUpgradeClicked?.Invoke();
	}
	void ClickMana2_3Upgrade()
	{
		onMana2_3UpgradeClicked?.Invoke();
	}

	void ClickRandomSpirit_1Upgrade()
	{
		onRandomSpirit_1UpgradeClicked?.Invoke();
	}
	#endregion
}