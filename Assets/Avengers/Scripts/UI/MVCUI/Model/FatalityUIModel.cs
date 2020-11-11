using Ruinarch.MVCFramework;
using System;
using UnityEngine;
using UnityEngine.UI;
public class FatalityUIModel : MVCUIModel
{
	public Action onSepticShockUpgradeClicked;
	public Action onHeartAttackUpgradeClicked;
	public Action onStrokeUpgradeClicked;
	public Action onTotalOrganFailureUpgradeClicked;
	public Action onPneumoniaUpgradeClicked;

	public Button btnSepticShockUpgrade;
	public Button btnHeartAttackUpgrade;
	public Button btnStrokeUpgrade;
	public Button btnTotalOrganFailureUpgrade;
	public Button btnPneumoniaUpgrade;

	private void OnEnable()
	{
		btnSepticShockUpgrade.onClick.AddListener(ClickSepticShockUpgrade);
		btnHeartAttackUpgrade.onClick.AddListener(ClickHeartAttackUpgrade);
		btnStrokeUpgrade.onClick.AddListener(ClickStrokeUpgrade);
		btnTotalOrganFailureUpgrade.onClick.AddListener(ClickTotalOrganFailureUpgrade);
		btnPneumoniaUpgrade.onClick.AddListener(ClickPneumoniaUpgrade);
	}

	private void OnDisable()
	{
		btnSepticShockUpgrade.onClick.RemoveListener(ClickSepticShockUpgrade);
		btnHeartAttackUpgrade.onClick.RemoveListener(ClickHeartAttackUpgrade);
		btnStrokeUpgrade.onClick.RemoveListener(ClickStrokeUpgrade);
		btnTotalOrganFailureUpgrade.onClick.RemoveListener(ClickTotalOrganFailureUpgrade);
		btnPneumoniaUpgrade.onClick.RemoveListener(ClickPneumoniaUpgrade);
	}

	#region Buttons OnClick trigger
	void ClickSepticShockUpgrade()
	{
		onSepticShockUpgradeClicked?.Invoke();
	}

	void ClickHeartAttackUpgrade()
	{
		onHeartAttackUpgradeClicked?.Invoke();
	}
	void ClickStrokeUpgrade()
	{
		onStrokeUpgradeClicked?.Invoke();
	}

	void ClickTotalOrganFailureUpgrade()
	{
		onTotalOrganFailureUpgradeClicked?.Invoke();
	}

	void ClickPneumoniaUpgrade()
	{
		onPneumoniaUpgradeClicked?.Invoke();
	}
	#endregion
}