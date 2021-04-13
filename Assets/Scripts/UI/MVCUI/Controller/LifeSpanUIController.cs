using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class LifeSpanUIController : MVCUIController, LifeSpanUIView.IListener
{
	[SerializeField]
	private LifeSpanUIModel m_lifeSpanUIModel;
	private LifeSpanUIView m_lifeSpanUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		LifeSpanUIView.Create(_canvas, m_lifeSpanUIModel, (p_ui) => {
			m_lifeSpanUIView = p_ui;
			m_lifeSpanUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}
	public override void ShowUI() {
		base.ShowUI();
		UpdateAllLifespanData();
	}

	private void UpdateAllLifespanData() {
		UpdateTileObjectInfectionTimeData();
		UpdateElvesInfectionTimeData();
		UpdateHumansInfectionTimeData();
		UpdateMonstersInfectionTimeData();
		UpdateUndeadInfectionTimeData();
	}
	
	#region LifeSpanUIView.IListener implementation
	public void OnObjectsUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetTileObjectInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeTileObjectInfectionTime();
		
		UpdateAllLifespanData();
	}
	public void OnElvesUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.ELVES);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeSapientInfectionTime(RACE.ELVES);
		
		UpdateAllLifespanData();
	}
	public void OnHumansUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.HUMANS);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeSapientInfectionTime(RACE.HUMANS);
		
		UpdateAllLifespanData();
	}
	public void OnMonstersUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetMonsterInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeMonsterInfectionTime();
		
		UpdateAllLifespanData();
	}
	public void OnUndeadUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetUndeadInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeUndeadInfectionTime();
		
		UpdateAllLifespanData();
	}
	public void OnObjectsHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip("Objects", "How long the Plague lasts on objects.", hoverPosition); }
	public void OnElvesHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip("Elves", "How long the Plague lasts on Elves.", hoverPosition); }
	public void OnHumansHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip("Humans", "How long the Plague lasts on Humans.", hoverPosition); }
	public void OnMonstersHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip("Monsters", "How long the Plague lasts on Monsters.", hoverPosition); }
	public void OnUndeadHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip("Undead", "How long the Plague lasts on Undead.", hoverPosition); }
	public void OnObjectsHoveredOut() { HideTooltip(); }
	public void OnElvesHoveredOut() { HideTooltip(); }
	public void OnHumansHoveredOut() { HideTooltip(); }
	public void OnMonstersHoveredOut() { HideTooltip(); }
	public void OnUndeadHoveredOut() { HideTooltip(); }
	public void OnUpgradeBtnObjectsHoveredOver() {
		m_lifeSpanUIView.UpdateTileObjectInfectionTime($"<color=\"green\">{PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedTileObjectInfectionTime())}</color>");
	}
	public void OnUpgradeBtnObjectsHoveredOut() {
		m_lifeSpanUIView.UpdateTileObjectInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.tileObjectInfectionTimeInHours));
	}
	public void OnUpgradeBtnElvesHoveredOver() {
		m_lifeSpanUIView.UpdateElvesInfectionTime($"<color=\"green\">{PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedSapientInfectionTime(RACE.ELVES))}</color>");
	}
	public void OnUpgradeBtnElvesHoveredOut() {
		m_lifeSpanUIView.UpdateElvesInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.ELVES)));
	}
	public void OnUpgradeBtnHumansHoveredOver() {
		m_lifeSpanUIView.UpdateHumansInfectionTime($"<color=\"green\">{PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedSapientInfectionTime(RACE.HUMANS))}</color>");
	}
	public void OnUpgradeBtnHumansHoveredOut() {
		m_lifeSpanUIView.UpdateHumansInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.HUMANS)));
	}
	public void OnUpgradeBtnMonstersHoveredOver() {
		m_lifeSpanUIView.UpdateMonstersInfectionTime($"<color=\"green\">{PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedMonsterInfectionTime())}</color>");
	}
	public void OnUpgradeBtnMonstersHoveredOut() {
		m_lifeSpanUIView.UpdateMonstersInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.monsterInfectionTimeInHours));
	}
	public void OnUpgradeBtnUndeadHoveredOver() {
		m_lifeSpanUIView.UpdateUndeadInfectionTime($"<color=\"green\">{PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedUndeadInfectionTime())}</color>");
	}
	public void OnUpgradeBtnUndeadHoveredOut() { 
		m_lifeSpanUIView.UpdateUndeadInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.undeadInfectionTimeInHours));
	}
	#endregion
	
	private void ShowTooltip(string p_lifespanHeader, string p_lifespanDescription, UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null) { UIManager.Instance.ShowSmallInfo(p_lifespanDescription, p_hoverPosition, p_lifespanHeader); }
	}
	private void HideTooltip() {
		if (UIManager.Instance != null) { UIManager.Instance.HideSmallInfo(); }
	}

	private bool CanAffordUpgrade(int cost) {
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= cost || (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None);
		} else {
			return true;
		}
	}
	
	private void UpdateTileObjectInfectionTimeData() {
		m_lifeSpanUIView.UpdateTileObjectInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.tileObjectInfectionTimeInHours));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetTileObjectInfectionTimeUpgradeCost();
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsTileObjectAtMaxLevel();
		m_lifeSpanUIView.UpdateTileObjectUpgradePrice(isAtMaxLevel ? "MAX" : $"{nextUpgradeCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_lifeSpanUIView.UpdateTileObjectUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
	}
	private void UpdateElvesInfectionTimeData() {
		m_lifeSpanUIView.UpdateElvesInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.ELVES)));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.ELVES);
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsSapientAtMaxLevel(RACE.ELVES);
		m_lifeSpanUIView.UpdateElvesUpgradePrice(isAtMaxLevel ? "MAX" : $"{nextUpgradeCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_lifeSpanUIView.UpdateElvesUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
	}
	private void UpdateHumansInfectionTimeData() {
		m_lifeSpanUIView.UpdateHumansInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.HUMANS)));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.HUMANS);
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsSapientAtMaxLevel(RACE.HUMANS);
		m_lifeSpanUIView.UpdateHumansUpgradePrice(isAtMaxLevel ? "MAX" : $"{nextUpgradeCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_lifeSpanUIView.UpdateHumansUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
	}
	private void UpdateMonstersInfectionTimeData() {
		m_lifeSpanUIView.UpdateMonstersInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.monsterInfectionTimeInHours));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetMonsterInfectionTimeUpgradeCost();
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsMonstersAtMaxLevel();
		m_lifeSpanUIView.UpdateMonstersUpgradePrice(isAtMaxLevel ? "MAX" : $"{nextUpgradeCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_lifeSpanUIView.UpdateMonstersUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
	}
	private void UpdateUndeadInfectionTimeData() {
		m_lifeSpanUIView.UpdateUndeadInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.undeadInfectionTimeInHours));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetUndeadInfectionTimeUpgradeCost();
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsUndeadAtMaxLevel();
		m_lifeSpanUIView.UpdateUndeadUpgradePrice(isAtMaxLevel ? "MAX" : $"{nextUpgradeCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
		m_lifeSpanUIView.UpdateUndeadUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
	}
}