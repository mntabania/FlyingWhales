using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LifeSpanUIModel : MVCUIModel
{
	public Action onObjectsUpgradeClicked;
	public Action onElvesUpgradeClicked;
	public Action onHumansUpgradeClicked;
	public Action onMonstersUpgradeClicked;
	public Action onUndeadUpgradeClicked;
	
	public Action<UIHoverPosition> onObjectsHoveredOver;
	public Action<UIHoverPosition> onElvesHoveredOver;
	public Action<UIHoverPosition> onHumansHoveredOver;
	public Action<UIHoverPosition> onMonstersHoveredOver;
	public Action<UIHoverPosition> onUndeadHoveredOver;
	
	public Action onObjectsHoveredOut;
	public Action onElvesHoveredOut;
	public Action onHumansHoveredOut;
	public Action onMonstersHoveredOut;
	public Action onUndeadHoveredOut;
	
	public Action onObjectsUpgradeBtnHoveredOver;
	public Action onElvesUpgradeBtnHoveredOver;
	public Action onHumansUpgradeBtnHoveredOver;
	public Action onMonstersUpgradeBtnHoveredOver;
	public Action onUndeadUpgradeBtnHoveredOver;
	
	public Action onObjectsUpgradeBtnHoveredOut;
	public Action onElvesUpgradeBtnHoveredOut;
	public Action onHumansUpgradeBtnHoveredOut;
	public Action onMonstersUpgradeBtnHoveredOut;
	public Action onUndeadUpgradeBtnHoveredOut;

	public RuinarchButton btnObjectsUpgrade;
	public RuinarchButton btnElvesUpgrade;
	public RuinarchButton btnHumansUpgrade;
	public RuinarchButton btnMonstersUpgrade;
	public RuinarchButton btnUndeadUpgrade;
	
	public TextMeshProUGUI txtObjectsUpgrade;
	public TextMeshProUGUI txtElvesUpgrade;
	public TextMeshProUGUI txtHumansUpgrade;
	public TextMeshProUGUI txtMonstersUpgrade;
	public TextMeshProUGUI txtUndeadUpgrade;

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

	public HoverHandler objectsHoverHandler;
	public HoverHandler elvesHoverHandler;
	public HoverHandler humansHoverHandler;
	public HoverHandler monstersHoverHandler;
	public HoverHandler undeadHoverHandler;

	public UIHoverPosition tooltipPosition;
	
	private void OnEnable() {
		btnObjectsUpgrade.onClick.AddListener(ClickObjectsUpgrade);
		btnElvesUpgrade.onClick.AddListener(ClickElvesUpgrade);
		btnHumansUpgrade.onClick.AddListener(ClickHumansUpgrade);
		btnMonstersUpgrade.onClick.AddListener(ClickMonstersUpgrade);
		btnUndeadUpgrade.onClick.AddListener(ClickUndeadUpgrade);
		objectsHoverHandler.AddOnHoverOverAction(OnHoverOverTileObject);
		objectsHoverHandler.AddOnHoverOutAction(OnHoverOutTileObject);
		elvesHoverHandler.AddOnHoverOverAction(OnHoverOverElves);
		elvesHoverHandler.AddOnHoverOutAction(OnHoverOutElves);
		humansHoverHandler.AddOnHoverOverAction(OnHoverOverHumans);
		humansHoverHandler.AddOnHoverOutAction(OnHoverOutHumans);
		monstersHoverHandler.AddOnHoverOverAction(OnHoverOverMonsters);
		monstersHoverHandler.AddOnHoverOutAction(OnHoverOutMonsters);
		undeadHoverHandler.AddOnHoverOverAction(OnHoverOverUndead);
		undeadHoverHandler.AddOnHoverOutAction(OnHoverOutUndead);
		btnObjectsUpgrade.AddHoverOverAction(OnHoverOverObjectsUpgradeBtn);
		btnElvesUpgrade.AddHoverOverAction(OnHoverOverElvesUpgradeBtn);
		btnHumansUpgrade.AddHoverOverAction(OnHoverOverHumansUpgradeBtn);
		btnMonstersUpgrade.AddHoverOverAction(OnHoverOverMonstersUpgradeBtn);
		btnUndeadUpgrade.AddHoverOverAction(OnHoverOverUndeadUpgradeBtn);
		btnObjectsUpgrade.AddHoverOutAction(OnHoverOutObjectsUpgradeBtn);
		btnElvesUpgrade.AddHoverOutAction(OnHoverOutElvesUpgradeBtn);
		btnHumansUpgrade.AddHoverOutAction(OnHoverOutHumansUpgradeBtn);
		btnMonstersUpgrade.AddHoverOutAction(OnHoverOutMonstersUpgradeBtn);
		btnUndeadUpgrade.AddHoverOutAction(OnHoverOutUndeadUpgradeBtn);
	}

	private void OnDisable() {
		btnObjectsUpgrade.onClick.RemoveListener(ClickObjectsUpgrade);
		btnElvesUpgrade.onClick.RemoveListener(ClickElvesUpgrade);
		btnHumansUpgrade.onClick.RemoveListener(ClickHumansUpgrade);
		btnMonstersUpgrade.onClick.RemoveListener(ClickMonstersUpgrade);
		btnUndeadUpgrade.onClick.RemoveListener(ClickUndeadUpgrade);
		objectsHoverHandler.RemoveOnHoverOverAction(OnHoverOverTileObject);
		objectsHoverHandler.RemoveOnHoverOutAction(OnHoverOutTileObject);
		elvesHoverHandler.RemoveOnHoverOverAction(OnHoverOverElves);
		elvesHoverHandler.RemoveOnHoverOutAction(OnHoverOutElves);
		humansHoverHandler.RemoveOnHoverOverAction(OnHoverOverHumans);
		humansHoverHandler.RemoveOnHoverOutAction(OnHoverOutHumans);
		monstersHoverHandler.RemoveOnHoverOverAction(OnHoverOverMonsters);
		monstersHoverHandler.RemoveOnHoverOutAction(OnHoverOutMonsters);
		undeadHoverHandler.RemoveOnHoverOverAction(OnHoverOverUndead);
		undeadHoverHandler.RemoveOnHoverOutAction(OnHoverOutUndead);
		btnObjectsUpgrade.RemoveHoverOverAction(OnHoverOverObjectsUpgradeBtn);
		btnElvesUpgrade.RemoveHoverOverAction(OnHoverOverElvesUpgradeBtn);
		btnHumansUpgrade.RemoveHoverOverAction(OnHoverOverHumansUpgradeBtn);
		btnMonstersUpgrade.RemoveHoverOverAction(OnHoverOverMonstersUpgradeBtn);
		btnUndeadUpgrade.RemoveHoverOverAction(OnHoverOverUndeadUpgradeBtn);
		btnObjectsUpgrade.RemoveHoverOutAction(OnHoverOutObjectsUpgradeBtn);
		btnElvesUpgrade.RemoveHoverOutAction(OnHoverOutElvesUpgradeBtn);
		btnHumansUpgrade.RemoveHoverOutAction(OnHoverOutHumansUpgradeBtn);
		btnMonstersUpgrade.RemoveHoverOutAction(OnHoverOutMonstersUpgradeBtn);
		btnUndeadUpgrade.RemoveHoverOutAction(OnHoverOutUndeadUpgradeBtn);
	}

	#region Buttons OnClick trigger
	void ClickObjectsUpgrade() {
		onObjectsUpgradeClicked?.Invoke();
	}
	void ClickElvesUpgrade() {
		onElvesUpgradeClicked?.Invoke();
	}
	void ClickHumansUpgrade() {
		onHumansUpgradeClicked?.Invoke();
	}
	void ClickMonstersUpgrade() {
		onMonstersUpgradeClicked?.Invoke();
	}
	void ClickUndeadUpgrade() {
		onUndeadUpgradeClicked?.Invoke();
	}
	#endregion

	#region On Hover
	void OnHoverOverTileObject() {
		onObjectsHoveredOver?.Invoke(tooltipPosition);
	}
	void OnHoverOutTileObject() {
		onObjectsHoveredOut?.Invoke();
	}
	void OnHoverOverElves() {
		onElvesHoveredOver?.Invoke(tooltipPosition);
	}
	void OnHoverOutElves() {
		onElvesHoveredOut?.Invoke();
	}
	void OnHoverOverHumans() {
		onHumansHoveredOver?.Invoke(tooltipPosition);
	}
	void OnHoverOutHumans() {
		onHumansHoveredOut?.Invoke();
	}
	void OnHoverOverMonsters() {
		onMonstersHoveredOver?.Invoke(tooltipPosition);
	}
	void OnHoverOutMonsters() {
		onMonstersHoveredOut?.Invoke();
	}
	void OnHoverOverUndead() {
		onUndeadHoveredOver?.Invoke(tooltipPosition);
	}
	void OnHoverOutUndead() {
		onUndeadHoveredOut?.Invoke();
	}
	void OnHoverOverObjectsUpgradeBtn() {
		onObjectsUpgradeBtnHoveredOver?.Invoke();
	}
	void OnHoverOverElvesUpgradeBtn() {
		onElvesUpgradeBtnHoveredOver?.Invoke();
	}
	void OnHoverOverHumansUpgradeBtn() {
		onHumansUpgradeBtnHoveredOver?.Invoke();
	}
	void OnHoverOverMonstersUpgradeBtn() {
		onMonstersUpgradeBtnHoveredOver?.Invoke();
	}
	void OnHoverOverUndeadUpgradeBtn() {
		onUndeadUpgradeBtnHoveredOver?.Invoke();
	}
	void OnHoverOutObjectsUpgradeBtn() {
		onObjectsUpgradeBtnHoveredOut?.Invoke();
	}
	void OnHoverOutElvesUpgradeBtn() {
		onElvesUpgradeBtnHoveredOut?.Invoke();
	}
	void OnHoverOutHumansUpgradeBtn() {
		onHumansUpgradeBtnHoveredOut?.Invoke();
	}
	void OnHoverOutMonstersUpgradeBtn() {
		onMonstersUpgradeBtnHoveredOut?.Invoke();
	}
	void OnHoverOutUndeadUpgradeBtn() {
		onUndeadUpgradeBtnHoveredOut?.Invoke();
	}
	#endregion
}