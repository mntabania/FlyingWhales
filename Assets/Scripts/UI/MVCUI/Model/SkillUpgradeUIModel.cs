using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class SkillUpgradeUIModel : MVCUIModel {
	public Action<bool> onAfflictionTabClicked;
	public Action<bool> onSpellTabClicked;
	public Action<bool> onPlayerActionTabClicked;
	public Action onCloseClicked;
	public Action<UIHoverPosition> onPlaguedRatsHoveredOver;
	public Action onPlaguedRatsHoveredOut;

	public RuinarchToggle btnAfflictionTab;
	public RuinarchToggle btnSpellTab;
	public RuinarchToggle btnPlayerActionTab;
	public Button btnClose;

	public RuinarchText txtTotalUnlocked;
	public RuinarchText txtChaoticEnergyAmount;

	public Transform tabPrent;
	public Transform skillParent;

	public UIHoverPosition tooltipPosition;

	private void OnEnable() {
		btnAfflictionTab.onValueChanged.AddListener(ClickAfflictionTab);
		btnSpellTab.onValueChanged.AddListener(ClickSpellTab);
		btnPlayerActionTab.onValueChanged.AddListener(ClickPlayerActionTab);
		btnClose.onClick.AddListener(ClickClose);
	}

	private void OnDisable() {
		btnAfflictionTab.onValueChanged.RemoveListener(ClickAfflictionTab);
		btnSpellTab.onValueChanged.RemoveListener(ClickSpellTab);
		btnPlayerActionTab.onValueChanged.RemoveListener(ClickPlayerActionTab);
		btnClose.onClick.RemoveListener(ClickClose);
	}

	#region Buttons OnClick trigger
	void ClickAfflictionTab(bool isOn) {
		onAfflictionTabClicked?.Invoke(isOn);
	}

	void ClickSpellTab(bool isOn) {
		onSpellTabClicked?.Invoke(isOn);
	}
	void ClickPlayerActionTab(bool isOn) {
		onPlayerActionTabClicked?.Invoke(isOn);
	}

	public void ClickClose() {
		onCloseClicked?.Invoke();
	}
	#endregion

	#region Hover Trigger
	void OnHoverOutPlaguedRats() {
		onPlaguedRatsHoveredOut?.Invoke();
	}
	#endregion
}