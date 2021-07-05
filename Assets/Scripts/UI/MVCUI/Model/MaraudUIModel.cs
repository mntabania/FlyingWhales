using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class MaraudUIModel : MVCUIModel {

	public Action onAddMinionClicked;
	public Action onAddTargetClicked;
	public Action onDeployClicked;
	public Action onCloseClicked;
	public Action onCloseSummonSubContainer;
	public Action onCloseMinionSubContainer;
	public Action onCloseTargetSubContainer;
	public Action onHoverOver;
	public Action onHoverOut;
	public Action onNoTargetsTipClicked;

	[Space]
	[Header("Deployed Items")]
	public List<DeployedMonsterItemUI> deployedItemSummonsUI = new List<DeployedMonsterItemUI>();
	public List<DeployedMonsterItemUI> deployedItemMinionsUI = new List<DeployedMonsterItemUI>();
	public List<DeployedTargetItemUI> deployedTargetItemUI = new List<DeployedTargetItemUI>();

	[Space]
	[Header("Buttons")]
	public RuinarchButton btnDeploy;
	public RuinarchButton btnClose;
	public RuinarchButton btnAddMinion;
	public RuinarchButton btnAddTarget;
	[Space]
	public RuinarchButton btnCloseSummonSubContainer;
	public RuinarchButton btnCloseMinionSubContainer;
	public RuinarchButton btnCloseTargetSubContainer;
	[Space]
	public RuinarchButton btnNoTargetsTip;
	[Space]
	public HoverHandler btnDeployHover;

	[Space]
	[Header("Scrollviews and Contents")]
	public Transform scrollViewDeployedSummons;
	public Transform scrollViewDeployedMinions;
	public Transform scrollViewDeployedTargets;
	public Transform availableSummonsParent;
	public Transform availableMinionsParent;
	public Transform deployedMinionsParent;
	public Transform deployedSummonsParent;
	public Transform availableTargetsParent;
	public Transform deployedTargetsParent;

	public GameObject subSummonContainer;
	public GameObject subMinionContainer;
	public GameObject subTargetContainer;

	public RuinarchText txtTitle;
	public RuinarchText txtEmptyTargetList;

	public UIHoverPosition hoverPosition;
	public UIHoverPosition deployedItemHoverPosition;

	private void OnEnable() {
		btnDeploy.onClick.AddListener(ClickDeploy);
		btnClose.onClick.AddListener(ClickClose);
		btnAddMinion.onClick.AddListener(ClickAddMinion);
		btnAddTarget.onClick.AddListener(ClickAddTarget);
		btnCloseSummonSubContainer.onClick.AddListener(ClickCloseSummonSubContainer);
		btnCloseMinionSubContainer.onClick.AddListener(ClickCloseMinionSubContainer);
		btnCloseTargetSubContainer.onClick.AddListener(ClickCloseTargetSubContainer);
		btnDeployHover.AddOnHoverOverAction(OnHoverOverDeployCursor);
		btnDeployHover.AddOnHoverOutAction(OnHoverOutDeployCursor);
		btnNoTargetsTip.onClick.AddListener(OnNoTargetsTipClicked);
	}

	private void OnDisable() {
		btnDeploy.onClick.RemoveListener(ClickDeploy);
		btnClose.onClick.RemoveListener(ClickClose);
		btnAddMinion.onClick.RemoveListener(ClickAddMinion);
		btnAddTarget.onClick.RemoveListener(ClickAddTarget);
		btnCloseSummonSubContainer.onClick.RemoveListener(ClickCloseSummonSubContainer);
		btnCloseMinionSubContainer.onClick.RemoveListener(ClickCloseMinionSubContainer);
		btnCloseTargetSubContainer.onClick.RemoveListener(ClickCloseTargetSubContainer);
		btnDeployHover.RemoveOnHoverOverAction(OnHoverOverDeployCursor);
		btnDeployHover.RemoveOnHoverOutAction(OnHoverOutDeployCursor);
		btnNoTargetsTip.onClick.RemoveListener(OnNoTargetsTipClicked);
	}

	#region Buttons OnClick trigger
	void ClickDeploy() {
		onDeployClicked?.Invoke();
	}

	public void ClickClose() {
		onCloseClicked?.Invoke();
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

	void ClickCloseMinionSubContainer() {
		onCloseMinionSubContainer?.Invoke();
	}

	void ClickCloseTargetSubContainer() {
		onCloseTargetSubContainer?.Invoke();
	}

	void OnHoverOverDeployCursor() {
		onHoverOver?.Invoke();
	}

	void OnHoverOutDeployCursor() {
		onHoverOut?.Invoke();
	}
	void OnNoTargetsTipClicked() {
		onNoTargetsTipClicked?.Invoke();
	}
	#endregion
}