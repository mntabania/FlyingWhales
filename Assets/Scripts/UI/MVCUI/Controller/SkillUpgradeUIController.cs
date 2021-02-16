using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Linq;
using Inner_Maps.Location_Structures;

public class SkillUpgradeUIController : MVCUIController, SkillUpgradeUIView.IListener {
	[SerializeField]
	private SkillUpgradeUIModel m_skillUpgradeUIModel;
	private SkillUpgradeUIView m_skillUpgradeUIView;

	public TransmissionUIController transmissionUIController;
	public LifeSpanUIController lifeSpanUIController;
	public FatalityUIController fatalityUIController;

	private Action onCloseUpgradeUI;

	public void Init(Action p_onCloseBiolabUI = null) {
		InstantiateUI();
		HideUI();
		if (p_onCloseBiolabUI != null) {
			onCloseUpgradeUI += p_onCloseBiolabUI;
		}
	}

	public void Open() {
		ShowUI();
		Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnPlaguePointsUpdated);
	}

	#region Listeners
	private void OnPlaguePointsUpdated(int p_plaguePoints) {
		UpdateTopMenuSummary();
	}
	#endregion

	public override void ShowUI() {
		base.ShowUI();
		UpdateTopMenuSummary();
		ShowUI(transmissionUIController);
		m_skillUpgradeUIView.SetTransmissionTabIsOnWithoutNotify(true);
	}
	public override void HideUI() {
		base.HideUI();
		onCloseUpgradeUI?.Invoke();
		Messenger.RemoveListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnPlaguePointsUpdated);
	}
	private void OnDisable() {
		
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		SkillUpgradeUIView.Create(_canvas, m_skillUpgradeUIModel, (p_ui) => {
			m_skillUpgradeUIView = p_ui;
			m_skillUpgradeUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);

			transmissionUIController.InstantiateUI();
			lifeSpanUIController.InstantiateUI();
			fatalityUIController.InstantiateUI();
		});
	}

	void LastUIInstantiated() {
		transmissionUIController.SetParent(m_skillUpgradeUIView.GetTabParentTransform());
		lifeSpanUIController.SetParent(m_skillUpgradeUIView.GetTabParentTransform());
		fatalityUIController.SetParent(m_skillUpgradeUIView.GetTabParentTransform());
		ShowUI(transmissionUIController);
		UpdateTopMenuSummary();
	}

	void ShowUI(MVCUIController p_targetUIToShow) {
		transmissionUIController.HideUI();
		lifeSpanUIController.HideUI();
		fatalityUIController.HideUI();

		p_targetUIToShow.ShowUI();
	}

	private void UpdateTopMenuSummary() {
		m_skillUpgradeUIView.SetActiveCases(PlagueDisease.Instance.activeCases.ToString());
		m_skillUpgradeUIView.SetDeathCases(PlagueDisease.Instance.deaths.ToString());
	}

	#region BiolabUIView.IListener implementation
	public void OnAfflictionTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(transmissionUIController);
		}
	}
	public void OnSpellTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(lifeSpanUIController);
		}
	}
	public void OnPlayerActionTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(fatalityUIController);
		}
	}
	
	public void OnCloseClicked() {
		HideUI();
	}
	public void OnHoveredOverPlaguedRat(UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null && PlayerManager.Instance != null) {
			if (PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.BIOLAB) is Biolab biolab && !biolab.HasMaxPlaguedRat()) {
				string timeDifference = GameManager.Instance.Today().GetTimeDifferenceString(biolab.replenishDate);
				string summary = $"The Biolab produces a Plagued Rat once every 2 days up to a maximum of \n3 charges. A new Plagued Rat charge will be produced in {UtilityScripts.Utilities.ColorizeAction(timeDifference)}.";
				UIManager.Instance.ShowSmallInfo(summary, p_hoverPosition, "Plagued Rats");
			}
		}
	}
	public void OnHoveredOutPlaguedRat() {
		if (UIManager.Instance != null && PlayerManager.Instance != null) {
			UIManager.Instance.HideSmallInfo();
		}
	}
	#endregion
}