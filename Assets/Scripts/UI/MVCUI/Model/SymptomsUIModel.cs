using Ruinarch.MVCFramework;
using System;
using UnityEngine;
using UnityEngine.UI;
public class SymptomsUIModel : MVCUIModel
{
	public Action onParalysisUpgradeClicked;
	public Action onVomitingUpgradeClicked;
	public Action onLethargyUpgradeClicked;
	public Action onSeizuresUpgradeClicked;
	public Action onInsomniaUpgradeClicked;
	public Action onPoisonCloudUpgradeClicked;
	public Action onMonsterScentUpgradeClicked;
	public Action onSneezingUpgradeClicked;
	public Action onDepressionUpgradeClicked;
	public Action onHungerPangsUpgradeClicked;

	public Button btnParalysisUpgrade;
	public Button btnVomitingUpgrade;
	public Button btnLethargyUpgrade;
	public Button btnSeizuresUpgrade;
	public Button btnInsomniaUpgrade;
	public Button btnPoisonCloudUpgrade;
	public Button btnMonsterScenetUpgrade;
	public Button btnSneezingUpgrade;
	public Button btnDepressionUpgrade;
	public Button btnHungerUpgrade;
	
	public RuinarchText txtParalysisCost;
	public RuinarchText txtVomitingCost;
	public RuinarchText txtLethargyCost;
	public RuinarchText txtSeizuresCost;
	public RuinarchText txtInsomniaCost;
	public RuinarchText txtPoisonCloudCost;
	public RuinarchText txtMonsterScentCost;
	public RuinarchText txtSneezingCost;
	public RuinarchText txtDepressionCost;
	public RuinarchText txtHungerCost;

	private void OnEnable()
	{
		btnParalysisUpgrade.onClick.AddListener(ClickParalysisUpgrade);
		btnVomitingUpgrade.onClick.AddListener(ClickVomitingUpgrade);
		btnLethargyUpgrade.onClick.AddListener(ClickLethargyUpgrade);
		btnSeizuresUpgrade.onClick.AddListener(ClickSeizuresUpgrade);
		btnInsomniaUpgrade.onClick.AddListener(ClickInsomniaUpgrade);
		btnPoisonCloudUpgrade.onClick.AddListener(ClickPoisonCloudUpgrade);
		btnMonsterScenetUpgrade.onClick.AddListener(ClickMonsterScentUpgrade);
		btnSneezingUpgrade.onClick.AddListener(ClickSneezingUpgrade);
		btnDepressionUpgrade.onClick.AddListener(ClickDepressionUpgrade);
		btnHungerUpgrade.onClick.AddListener(ClickHungerPangsUpgrade);
	}

	private void OnDisable()
	{
		btnParalysisUpgrade.onClick.RemoveListener(ClickParalysisUpgrade);
		btnVomitingUpgrade.onClick.RemoveListener(ClickVomitingUpgrade);
		btnLethargyUpgrade.onClick.RemoveListener(ClickLethargyUpgrade);
		btnSeizuresUpgrade.onClick.RemoveListener(ClickSeizuresUpgrade);
		btnInsomniaUpgrade.onClick.RemoveListener(ClickInsomniaUpgrade);
		btnPoisonCloudUpgrade.onClick.RemoveListener(ClickPoisonCloudUpgrade);
		btnMonsterScenetUpgrade.onClick.RemoveListener(ClickMonsterScentUpgrade);
		btnSneezingUpgrade.onClick.RemoveListener(ClickSneezingUpgrade);
		btnDepressionUpgrade.onClick.RemoveListener(ClickDepressionUpgrade);
		btnHungerUpgrade.onClick.RemoveListener(ClickHungerPangsUpgrade);
	}

	#region Buttons OnClick trigger
	void ClickParalysisUpgrade()
	{
		onParalysisUpgradeClicked?.Invoke();
	}

	void ClickVomitingUpgrade()
	{
		onVomitingUpgradeClicked?.Invoke();
	}
	void ClickLethargyUpgrade()
	{
		onLethargyUpgradeClicked?.Invoke();
	}

	void ClickSeizuresUpgrade()
	{
		onSeizuresUpgradeClicked?.Invoke();
	}

	void ClickInsomniaUpgrade()
	{
		onInsomniaUpgradeClicked?.Invoke();
	}

	void ClickPoisonCloudUpgrade()
	{
		onPoisonCloudUpgradeClicked?.Invoke();
	}

	void ClickMonsterScentUpgrade()
	{
		onMonsterScentUpgradeClicked?.Invoke();
	}
	void ClickSneezingUpgrade()
	{
		onSneezingUpgradeClicked?.Invoke();
	}

	void ClickDepressionUpgrade()
	{
		onDepressionUpgradeClicked?.Invoke();
	}

	void ClickHungerPangsUpgrade()
	{
		onHungerPangsUpgradeClicked?.Invoke();
	}
	#endregion
}