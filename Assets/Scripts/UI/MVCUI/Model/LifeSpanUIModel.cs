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
	#endregion
}