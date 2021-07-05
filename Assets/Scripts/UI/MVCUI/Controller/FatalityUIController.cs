using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Plague.Fatality;

public class FatalityUIController : MVCUIController, FatalityUIView.IListener
{
	[SerializeField]
	private FatalityUIModel m_fatalityUIModel;
	private FatalityUIView m_fatalityUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		FatalityUIView.Create(_canvas, m_fatalityUIModel, (p_ui) => {
			m_fatalityUIView = p_ui;
			m_fatalityUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}
	public override void ShowUI() {
		base.ShowUI();
		UpdateAllFatalityData();
	}
	private void UpdateAllFatalityData() {
		UpdateSepticShockData();
        UpdateHeartAttackData();
        UpdateStrokeData();
        UpdateTotalOrganFailureData();
        UpdatePneumoniaData();
	}

	#region FatalityUIView.IListener implementation
	public void OnSepticShockUpgradeClicked() {
		PayForFatality(PLAGUE_FATALITY.Septic_Shock);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Septic_Shock);
		UpdateAllFatalityData();
	}
	public void OnHeartAttackUpgradeClicked() {
		PayForFatality(PLAGUE_FATALITY.Heart_Attack);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Heart_Attack);
		UpdateAllFatalityData();
	}
	public void OnStrokeUpgradeClicked() {
		PayForFatality(PLAGUE_FATALITY.Stroke);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Stroke);
		UpdateAllFatalityData();
	}
	public void OnTotalOrganFailureUpgradeClicked() {
		PayForFatality(PLAGUE_FATALITY.Total_Organ_Failure);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Total_Organ_Failure);
		UpdateAllFatalityData();
	}
	public void OnPneumoniaUpgradeClicked() {
		PayForFatality(PLAGUE_FATALITY.Pneumonia);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Pneumonia);
		UpdateAllFatalityData();
	}
	public void OnSepticShockHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_FATALITY.Septic_Shock, hoverPosition); }
	public void OnSepticShockHoveredOut() { HideTooltip(); }
	public void OnHeartAttackHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_FATALITY.Heart_Attack, hoverPosition); }
	public void OnHeartAttackHoveredOut() { HideTooltip(); }
	public void OnStrokeHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_FATALITY.Stroke, hoverPosition); }
	public void OnStrokeHoveredOut() { HideTooltip(); }
	public void OnTotalOrganFailureHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_FATALITY.Total_Organ_Failure, hoverPosition); }
	public void OnTotalOrganFailureHoveredOut() { HideTooltip(); }
	public void OnPneumoniaHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_FATALITY.Pneumonia, hoverPosition); }
	public void OnPneumoniaHoveredOut() { HideTooltip(); }
	#endregion

	private void ShowTooltip(PLAGUE_FATALITY p_fatalityType, UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null) { UIManager.Instance.ShowSmallInfo(p_fatalityType.GetFatalityTooltip(), p_hoverPosition, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_fatalityType.ToString())); }
	}
	private void HideTooltip() {
		if (UIManager.Instance != null) { UIManager.Instance.HideSmallInfo(); }
	}
	
	private void UpdateSepticShockData() {
		bool hasUnlockedFatality = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Septic_Shock);
		m_fatalityUIView.UpdateSepticShockCost($"{PLAGUE_FATALITY.Septic_Shock.GetFatalityCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_fatalityUIView.UpdateSepticShockCostState(!hasUnlockedFatality);
		m_fatalityUIView.UpdateSepticShockUpgradeButtonInteractable(!hasUnlockedFatality && !PlagueDisease.Instance.HasActivatedMaxFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Septic_Shock));
		m_fatalityUIView.UpdateSepticShockCheckmarkState(hasUnlockedFatality);
	}
	private void UpdateHeartAttackData() {
		bool hasUnlockedFatality = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Heart_Attack);
		m_fatalityUIView.UpdateHeartAttackCost($"{PLAGUE_FATALITY.Heart_Attack.GetFatalityCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_fatalityUIView.UpdateHeartAttackCostState(!hasUnlockedFatality);
		m_fatalityUIView.UpdateHeartAttackUpgradeButtonInteractable(!hasUnlockedFatality && !PlagueDisease.Instance.HasActivatedMaxFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Heart_Attack));
		m_fatalityUIView.UpdateHeartAttackCheckmarkState(hasUnlockedFatality);
	}
	private void UpdateStrokeData() {
		bool hasUnlockedFatality = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Stroke);
		m_fatalityUIView.UpdateStrokeCost($"{PLAGUE_FATALITY.Stroke.GetFatalityCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_fatalityUIView.UpdateStrokeCostState(!hasUnlockedFatality);
		m_fatalityUIView.UpdateStrokeUpgradeButtonInteractable(!hasUnlockedFatality && !PlagueDisease.Instance.HasActivatedMaxFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Stroke));
		m_fatalityUIView.UpdateStrokeCheckmarkState(hasUnlockedFatality);
	}
	private void UpdateTotalOrganFailureData() {
		bool hasUnlockedFatality = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Total_Organ_Failure);
		m_fatalityUIView.UpdateTotalOrganFailureCost($"{PLAGUE_FATALITY.Total_Organ_Failure.GetFatalityCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_fatalityUIView.UpdateTotalOrganFailureCostState(!hasUnlockedFatality);
		m_fatalityUIView.UpdateTotalOrganFailureUpgradeButtonInteractable(!hasUnlockedFatality && !PlagueDisease.Instance.HasActivatedMaxFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Total_Organ_Failure));
		m_fatalityUIView.UpdateTotalOrganFailureCheckmarkState(hasUnlockedFatality);
	}
	private void UpdatePneumoniaData() {
		bool hasUnlockedFatality = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Pneumonia);
		m_fatalityUIView.UpdatePneumoniaCost($"{PLAGUE_FATALITY.Pneumonia.GetFatalityCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_fatalityUIView.UpdatePneumoniaCostState(!hasUnlockedFatality);
		m_fatalityUIView.UpdatePneumoniaUpgradeButtonInteractable(!hasUnlockedFatality && !PlagueDisease.Instance.HasActivatedMaxFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Pneumonia));
		m_fatalityUIView.UpdatePneumoniaCheckmarkState(hasUnlockedFatality);
	}

	private void PayForFatality(PLAGUE_FATALITY p_fatalityType) {
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-p_fatalityType.GetFatalityCost());
		}
	}
	private bool CanAffordSymptom(PLAGUE_FATALITY p_fatalityType) {
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= p_fatalityType.GetFatalityCost() || (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None);
		} else {
			return true;
		}
	}
}