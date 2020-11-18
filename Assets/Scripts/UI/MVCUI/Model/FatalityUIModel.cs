using Ruinarch.MVCFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FatalityUIModel : MVCUIModel
{
	public Action onSepticShockUpgradeClicked;
	public Action onHeartAttackUpgradeClicked;
	public Action onStrokeUpgradeClicked;
	public Action onTotalOrganFailureUpgradeClicked;
	public Action onPneumoniaUpgradeClicked;

	public Action<UIHoverPosition> onSepticShockHoveredOver;
	public Action<UIHoverPosition> onHeartAttackHoveredOver;
	public Action<UIHoverPosition> onStrokeHoveredOver;
	public Action<UIHoverPosition> onTotalOrganFailureHoveredOver;
	public Action<UIHoverPosition> onPneumoniaHoveredOver;
	
	public Action onSepticShockHoveredOut;
	public Action onHeartAttackHoveredOut;
	public Action onStrokeHoveredOut;
	public Action onTotalOrganFailureHoveredOut;
	public Action onPneumoniaHoveredOut;

	public Button btnSepticShockUpgrade;
	public Button btnHeartAttackUpgrade;
	public Button btnStrokeUpgrade;
	public Button btnTotalOrganFailureUpgrade;
	public Button btnPneumoniaUpgrade;
	
	public TextMeshProUGUI txtSepticShockUpgrade;
	public TextMeshProUGUI txtHeartAttackUpgrade;
	public TextMeshProUGUI txtStrokeUpgrade;
	public TextMeshProUGUI txtTotalOrganFailureUpgrade;
	public TextMeshProUGUI txtPneumoniaUpgrade;

	public RuinarchText txtSepticShockCost;
	public RuinarchText txtHeartAttackCost;
	public RuinarchText txtStrokeCost;
	public RuinarchText txtTotalOrganFailureCost;
	public RuinarchText txtPneumoniaCost;

	public HoverHandler septicShockHoverHandler;
	public HoverHandler heartAttackHoverHandler;
	public HoverHandler strokeHoverHandler;
	public HoverHandler totalOrganFailureHoverHandler;
	public HoverHandler pneumoniaHoverHandler;

	public UIHoverPosition hoverPosition;
	
	private void OnEnable() {
		btnSepticShockUpgrade.onClick.AddListener(ClickSepticShockUpgrade);
		btnHeartAttackUpgrade.onClick.AddListener(ClickHeartAttackUpgrade);
		btnStrokeUpgrade.onClick.AddListener(ClickStrokeUpgrade);
		btnTotalOrganFailureUpgrade.onClick.AddListener(ClickTotalOrganFailureUpgrade);
		btnPneumoniaUpgrade.onClick.AddListener(ClickPneumoniaUpgrade);
		septicShockHoverHandler.AddOnHoverOverAction(OnHoverOverSepticShock);
		septicShockHoverHandler.AddOnHoverOutAction(OnHoverOutSepticShock);
		heartAttackHoverHandler.AddOnHoverOverAction(OnHoverOverHeartAttack);
		heartAttackHoverHandler.AddOnHoverOutAction(OnHoverOutHeartAttack);
		strokeHoverHandler.AddOnHoverOverAction(OnHoverOverStroke);
		strokeHoverHandler.AddOnHoverOutAction(OnHoverOutStroke);
		totalOrganFailureHoverHandler.AddOnHoverOverAction(OnHoverOverTotalOrganFailure);
		totalOrganFailureHoverHandler.AddOnHoverOutAction(OnHoverOutTotalOrganFailure);
		pneumoniaHoverHandler.AddOnHoverOverAction(OnHoverOverPneumonia);
		pneumoniaHoverHandler.AddOnHoverOutAction(OnHoverOutPneumonia);
	}

	private void OnDisable() {
		btnSepticShockUpgrade.onClick.RemoveListener(ClickSepticShockUpgrade);
		btnHeartAttackUpgrade.onClick.RemoveListener(ClickHeartAttackUpgrade);
		btnStrokeUpgrade.onClick.RemoveListener(ClickStrokeUpgrade);
		btnTotalOrganFailureUpgrade.onClick.RemoveListener(ClickTotalOrganFailureUpgrade);
		btnPneumoniaUpgrade.onClick.RemoveListener(ClickPneumoniaUpgrade);
		septicShockHoverHandler.RemoveOnHoverOverAction(OnHoverOverSepticShock);
		septicShockHoverHandler.RemoveOnHoverOutAction(OnHoverOutSepticShock);
	}

	#region Buttons OnClick trigger
	void ClickSepticShockUpgrade() {
		onSepticShockUpgradeClicked?.Invoke();
	}

	void ClickHeartAttackUpgrade() {
		onHeartAttackUpgradeClicked?.Invoke();
	}
	void ClickStrokeUpgrade() {
		onStrokeUpgradeClicked?.Invoke();
	}
	void ClickTotalOrganFailureUpgrade() {
		onTotalOrganFailureUpgradeClicked?.Invoke();
	}

	void ClickPneumoniaUpgrade() {
		onPneumoniaUpgradeClicked?.Invoke();
	}
	#endregion

	#region On Hover Trigger
	void OnHoverOverSepticShock() {
		onSepticShockHoveredOver?.Invoke(hoverPosition);
	}
	void OnHoverOutSepticShock() {
		onSepticShockHoveredOut?.Invoke();
	}
	void OnHoverOverHeartAttack() {
		onHeartAttackHoveredOver?.Invoke(hoverPosition);
	}
	void OnHoverOutHeartAttack() {
		onHeartAttackHoveredOut?.Invoke();
	}
	void OnHoverOverStroke() {
		onStrokeHoveredOver?.Invoke(hoverPosition);
	}
	void OnHoverOutStroke() {
		onStrokeHoveredOut?.Invoke();
	}
	void OnHoverOverTotalOrganFailure() {
		onTotalOrganFailureHoveredOver?.Invoke(hoverPosition);
	}
	void OnHoverOutTotalOrganFailure() {
		onTotalOrganFailureHoveredOut?.Invoke();
	}
	void OnHoverOverPneumonia() {
		onPneumoniaHoveredOver?.Invoke(hoverPosition);
	}
	void OnHoverOutPneumonia() {
		onPneumoniaHoveredOut?.Invoke();
	}
	#endregion
}