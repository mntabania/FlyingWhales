using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Plague.Symptom;

public class SymptomsUIController : MVCUIController, SymptomsUIView.IListener
{
	[SerializeField]
	private SymptomsUIModel m_symptomsUIModel;
	private SymptomsUIView m_symptomsUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SymptomsUIView.Create(_canvas, m_symptomsUIModel, (p_ui) => {
			m_symptomsUIView = p_ui;
			m_symptomsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}
	public override void ShowUI() {
		base.ShowUI();
		UpdateAllSymptomsData();
	}
	private void UpdateAllSymptomsData() {
		UpdateSymptomData(PLAGUE_SYMPTOM.Paralysis);
		UpdateSymptomData(PLAGUE_SYMPTOM.Vomiting);
		UpdateSymptomData(PLAGUE_SYMPTOM.Lethargy);
		UpdateSymptomData(PLAGUE_SYMPTOM.Seizure);
		UpdateSymptomData(PLAGUE_SYMPTOM.Insomnia);
		UpdateSymptomData(PLAGUE_SYMPTOM.Poison_Cloud);
		UpdateSymptomData(PLAGUE_SYMPTOM.Monster_Scent);
		UpdateSymptomData(PLAGUE_SYMPTOM.Sneezing);
		UpdateSymptomData(PLAGUE_SYMPTOM.Depression);
		UpdateSymptomData(PLAGUE_SYMPTOM.Hunger_Pangs);
	}
	
	private void UpdateSymptomData(PLAGUE_SYMPTOM p_symptomType) {
		bool hasUnlockedSymptom = PlagueDisease.Instance.IsSymptomActive(p_symptomType);
		m_symptomsUIView.UpdateSymptomCost(p_symptomType, $"{p_symptomType.GetSymptomCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_symptomsUIView.UpdateSymptomCostState(p_symptomType, !hasUnlockedSymptom);
		m_symptomsUIView.UpdateSymptomUpgradeButtonInteractable(p_symptomType, !hasUnlockedSymptom && !PlagueDisease.Instance.HasActivatedMaxSymptoms() && CanAffordSymptom(p_symptomType));
		m_symptomsUIView.UpdateSymptomCheckmarkState(p_symptomType, hasUnlockedSymptom);
	}

	#region SymptomsUIView.IListener implementation
	public void OnParalysisUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Paralysis);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Paralysis);
		UpdateAllSymptomsData();
	}
	public void OnVomitingUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Vomiting);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Vomiting);
		UpdateAllSymptomsData();
	}
	public void OnLethargyUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Lethargy);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Lethargy);
		UpdateAllSymptomsData();
	}
	public void OnSeizuresUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Seizure);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Seizure);
		UpdateAllSymptomsData();
	}
	public void OnInsomniaUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Insomnia);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Insomnia);
		UpdateAllSymptomsData();
	}
	public void OnPoisonCloudUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Poison_Cloud);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Poison_Cloud);
		UpdateAllSymptomsData();
	}
	public void OnMonsterScentUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Monster_Scent);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Monster_Scent);
		UpdateAllSymptomsData();
	}
	public void OnSneezingUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Sneezing);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Sneezing);
		UpdateAllSymptomsData();
	}
	public void OnDepressionUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Depression);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Depression);
		UpdateAllSymptomsData();
	}
	public void OnHungerPangsUpgradeClicked() {
		PayForSymptom(PLAGUE_SYMPTOM.Hunger_Pangs);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Hunger_Pangs);
		UpdateAllSymptomsData();
	}
	public void OnHoverOverParalysis(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Paralysis, p_hoverPosition); }
	public void OnHoverOverVomiting(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Vomiting, p_hoverPosition); }
	public void OnHoverOverLethargy(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Lethargy, p_hoverPosition); }
	public void OnHoverOverSeizures(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Seizure, p_hoverPosition); }
	public void OnHoverOverInsomnia(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Insomnia, p_hoverPosition); }
	public void OnHoverOverPoisonCloud(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Poison_Cloud, p_hoverPosition); }
	public void OnHoverOverMonsterScent(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Monster_Scent, p_hoverPosition); }
	public void OnHoverOverSneezing(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Sneezing, p_hoverPosition); }
	public void OnHoverOverDepression(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Depression, p_hoverPosition); }
	public void OnHoverOverHungerPangs(UIHoverPosition p_hoverPosition) { ShowTooltip(PLAGUE_SYMPTOM.Hunger_Pangs, p_hoverPosition); }
	public void OnHoverOutParalysis() { HideTooltip(); }
	public void OnHoverOutVomiting() { HideTooltip(); }
	public void OnHoverOutLethargy() { HideTooltip(); }
	public void OnHoverOutSeizures() { HideTooltip(); }
	public void OnHoverOutInsomnia() { HideTooltip(); }
	public void OnHoverOutPoisonCloud() { HideTooltip(); }
	public void OnHoverOutMonsterScent() { HideTooltip(); }
	public void OnHoverOutSneezing() { HideTooltip(); }
	public void OnHoverOutDepression() { HideTooltip(); }
	public void OnHoverOutHungerPangs() { HideTooltip(); }
	#endregion

	private void PayForSymptom(PLAGUE_SYMPTOM p_symptomType) {
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-p_symptomType.GetSymptomCost());
		}
	}
	private bool CanAffordSymptom(PLAGUE_SYMPTOM p_symptomType) {
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= p_symptomType.GetSymptomCost() || (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None);
		} else {
			return true;
		}
	}
	private void ShowTooltip(PLAGUE_SYMPTOM p_symptomType, UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null) { UIManager.Instance.ShowSmallInfo(p_symptomType.GetSymptomTooltip(), p_hoverPosition, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_symptomType.ToString())); }
	}
	private void HideTooltip() {
		if (UIManager.Instance != null) { UIManager.Instance.HideSmallInfo(); }
	}
}