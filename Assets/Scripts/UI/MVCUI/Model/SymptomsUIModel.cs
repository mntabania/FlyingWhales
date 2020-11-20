using Ruinarch.MVCFramework;
using System;
using TMPro;
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
	
	public Action<UIHoverPosition> onParalysisHoveredOver;
	public Action<UIHoverPosition> onVomitingHoveredOver;
	public Action<UIHoverPosition> onLethargyHoveredOver;
	public Action<UIHoverPosition> onSeizuresHoveredOver;
	public Action<UIHoverPosition> onInsomniaHoveredOver;
	public Action<UIHoverPosition> onPoisonCloudHoveredOver;
	public Action<UIHoverPosition> onMonsterScentHoveredOver;
	public Action<UIHoverPosition> onSneezingHoveredOver;
	public Action<UIHoverPosition> onDepressionHoveredOver;
	public Action<UIHoverPosition> onHungerPangsHoveredOver;
	
	public Action onParalysisHoveredOut;
	public Action onVomitingHoveredOut;
	public Action onLethargyHoveredOut;
	public Action onSeizuresHoveredOut;
	public Action onInsomniaHoveredOut;
	public Action onPoisonCloudHoveredOut;
	public Action onMonsterScentHoveredOut;
	public Action onSneezingHoveredOut;
	public Action onDepressionHoveredOut;
	public Action onHungerPangsHoveredOut;

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
	
	public TextMeshProUGUI txtParalysisUpgrade;
	public TextMeshProUGUI txtVomitingUpgrade;
	public TextMeshProUGUI txtLethargyUpgrade;
	public TextMeshProUGUI txtSeizuresUpgrade;
	public TextMeshProUGUI txtInsomniaUpgrade;
	public TextMeshProUGUI txtPoisonCloudUpgrade;
	public TextMeshProUGUI txtMonsterScenetUpgrade;
	public TextMeshProUGUI txtSneezingUpgrade;
	public TextMeshProUGUI txtDepressionUpgrade;
	public TextMeshProUGUI txtHungerUpgrade;
	
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

	public HoverHandler paralysisHoverHandler;
	public HoverHandler vomitingHoverHandler;
	public HoverHandler lethargyHoverHandler;
	public HoverHandler seizuresHoverHandler;
	public HoverHandler insomniaHoverHandler;
	public HoverHandler poisonCloudHoverHandler;
	public HoverHandler monsterScenetHoverHandler;
	public HoverHandler sneezingHoverHandler;
	public HoverHandler depressionHoverHandler;
	public HoverHandler hungerHoverHandler;
	
	public GameObject checkMarkParalysisUpgrade;
	public GameObject checkMarkVomitingUpgrade;
	public GameObject checkMarkLethargyUpgrade;
	public GameObject checkMarkSeizuresUpgrade;
	public GameObject checkMarkInsomniaUpgrade;
	public GameObject checkMarkPoisonCloudUpgrade;
	public GameObject checkMarkMonsterScenetUpgrade;
	public GameObject checkMarkSneezingUpgrade;
	public GameObject checkMarkDepressionUpgrade;
	public GameObject checkMarkHungerUpgrade;

	public UIHoverPosition tooltipPosition;
	
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
		paralysisHoverHandler.AddOnHoverOverAction(OnHoverOverParalysis);
		vomitingHoverHandler.AddOnHoverOverAction(OnHoverOverVomit);
		lethargyHoverHandler.AddOnHoverOverAction(OnHoverOverLethargy);
		seizuresHoverHandler.AddOnHoverOverAction(OnHoverOverSeizure);
		insomniaHoverHandler.AddOnHoverOverAction(OnHoverOverInsomnia);
		poisonCloudHoverHandler.AddOnHoverOverAction(OnHoverOverPoisonCloud);
		monsterScenetHoverHandler.AddOnHoverOverAction(OnHoverOverMonsterScent);
		sneezingHoverHandler.AddOnHoverOverAction(OnHoverOverSneezing);
		depressionHoverHandler.AddOnHoverOverAction(OnHoverOverDepression);
		hungerHoverHandler.AddOnHoverOverAction(OnHoverOverHunger);
		paralysisHoverHandler.AddOnHoverOutAction(OnHoverOutParalysis);
		vomitingHoverHandler.AddOnHoverOutAction(OnHoverOutVomit);
		lethargyHoverHandler.AddOnHoverOutAction(OnHoverOutLethargy);
		seizuresHoverHandler.AddOnHoverOutAction(OnHoverOutSeizure);
		insomniaHoverHandler.AddOnHoverOutAction(OnHoverOutInsomnia);
		poisonCloudHoverHandler.AddOnHoverOutAction(OnHoverOutPoisonCloud);
		monsterScenetHoverHandler.AddOnHoverOutAction(OnHoverOutMonsterScent);
		sneezingHoverHandler.AddOnHoverOutAction(OnHoverOutSneezing);
		depressionHoverHandler.AddOnHoverOutAction(OnHoverOutDepression);
		hungerHoverHandler.AddOnHoverOutAction(OnHoverOutHunger);
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
		paralysisHoverHandler.RemoveOnHoverOverAction(OnHoverOverParalysis);
		vomitingHoverHandler.RemoveOnHoverOverAction(OnHoverOverVomit);
		lethargyHoverHandler.RemoveOnHoverOverAction(OnHoverOverLethargy);
		seizuresHoverHandler.RemoveOnHoverOverAction(OnHoverOverSeizure);
		insomniaHoverHandler.RemoveOnHoverOverAction(OnHoverOverInsomnia);
		poisonCloudHoverHandler.RemoveOnHoverOverAction(OnHoverOverPoisonCloud);
		monsterScenetHoverHandler.RemoveOnHoverOverAction(OnHoverOverMonsterScent);
		sneezingHoverHandler.RemoveOnHoverOverAction(OnHoverOverSneezing);
		depressionHoverHandler.RemoveOnHoverOverAction(OnHoverOverDepression);
		hungerHoverHandler.RemoveOnHoverOverAction(OnHoverOverHunger);
		paralysisHoverHandler.RemoveOnHoverOutAction(OnHoverOutParalysis);
		vomitingHoverHandler.RemoveOnHoverOutAction(OnHoverOutVomit);
		lethargyHoverHandler.RemoveOnHoverOutAction(OnHoverOutLethargy);
		seizuresHoverHandler.RemoveOnHoverOutAction(OnHoverOutSeizure);
		insomniaHoverHandler.RemoveOnHoverOutAction(OnHoverOutInsomnia);
		poisonCloudHoverHandler.RemoveOnHoverOutAction(OnHoverOutPoisonCloud);
		monsterScenetHoverHandler.RemoveOnHoverOutAction(OnHoverOutMonsterScent);
		sneezingHoverHandler.RemoveOnHoverOutAction(OnHoverOutSneezing);
		depressionHoverHandler.RemoveOnHoverOutAction(OnHoverOutDepression);
		hungerHoverHandler.RemoveOnHoverOutAction(OnHoverOutHunger);
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

	#region On Hover
	private void OnHoverOverParalysis(){ onParalysisHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverVomit(){ onVomitingHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverLethargy(){ onLethargyHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverSeizure(){ onSeizuresHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverInsomnia(){ onInsomniaHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverPoisonCloud(){ onPoisonCloudHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverMonsterScent(){ onMonsterScentHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverSneezing(){ onSneezingHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverDepression(){ onDepressionHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOverHunger(){ onHungerPangsHoveredOver?.Invoke(tooltipPosition); }
	private void OnHoverOutParalysis(){ onParalysisHoveredOut?.Invoke(); }
	private void OnHoverOutVomit(){ onVomitingHoveredOut?.Invoke(); }
	private void OnHoverOutLethargy(){ onLethargyHoveredOut?.Invoke(); }
	private void OnHoverOutSeizure(){ onSeizuresHoveredOut?.Invoke(); }
	private void OnHoverOutInsomnia(){ onInsomniaHoveredOut?.Invoke(); }
	private void OnHoverOutPoisonCloud(){ onPoisonCloudHoveredOut?.Invoke(); }
	private void OnHoverOutMonsterScent(){ onMonsterScentHoveredOut?.Invoke(); }
	private void OnHoverOutSneezing(){ onSneezingHoveredOut?.Invoke(); }
	private void OnHoverOutDepression(){ onDepressionHoveredOut?.Invoke(); }
	private void OnHoverOutHunger(){ onHungerPangsHoveredOut?.Invoke(); }
	#endregion
}