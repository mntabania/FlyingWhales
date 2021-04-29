using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DefensePointUIModel : MVCUIModel {

	public Action onAddMinionClicked;
	public Action onAddSummonClicked;
	public Action onAddTargetClicked;
	public Action onDeployClicked;
	public Action onCloseClicked;
	public Action onCloseSummonSubContainer;
	public Action onHoverOver;
	public Action onHoverOut;

	public List<DeployedMonsterItemUI> deployedItemSummonsUI = new List<DeployedMonsterItemUI>();

	[Space]
	[Header("Buttons")]
	public Button btnDeploy;
	public Button btnClose;
	[Space]
	public Button btnCloseSummonSubContainer;
	[Space]
	public HoverHandler btnDeployHover;

	[Space]
	[Header("Scrollviews and Contents")]
	public Transform scrollViewDeployedSummons;
	public Transform availableSummonsParent;

	public GameObject subSummonContainer;
	
	public RuinarchText txtTitle;

	public UIHoverPosition hoverPosition;
	private void OnEnable() {
		btnDeploy.onClick.AddListener(ClickDeploy);
		btnClose.onClick.AddListener(ClickClose);
		btnCloseSummonSubContainer.onClick.AddListener(ClickCloseSummonSubContainer);
		btnDeployHover.AddOnHoverOverAction(OnHoverOverDeployCursor);
		btnDeployHover.AddOnHoverOutAction(OnHoverOutDeployCursor);
	}

	private void OnDisable() {
		btnDeploy.onClick.RemoveListener(ClickDeploy);
		btnClose.onClick.RemoveListener(ClickClose);
		btnCloseSummonSubContainer.onClick.RemoveListener(ClickCloseSummonSubContainer);
		btnDeployHover.RemoveOnHoverOverAction(OnHoverOverDeployCursor);
		btnDeployHover.RemoveOnHoverOutAction(OnHoverOutDeployCursor);
	}

	#region Buttons OnClick trigger
	void ClickDeploy() {
		onDeployClicked?.Invoke();
	}

	public void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickAddSummon() {
		onAddSummonClicked?.Invoke();
	}

	void ClickAddMinion() {
		onAddMinionClicked?.Invoke();
	}

	void ClickAddTarget() {
		onAddTargetClicked?.Invoke();
	}

	void ClickCloseSummonSubContainer() {
		onCloseSummonSubContainer?.Invoke();
	}

	void OnHoverOverDeployCursor() {
		onHoverOver?.Invoke();
	}

	void OnHoverOutDeployCursor() {
		onHoverOut?.Invoke();
	}
	#endregion
}