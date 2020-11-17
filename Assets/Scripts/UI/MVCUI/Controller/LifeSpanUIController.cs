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
		
		UpdateTileObjectInfectionTimeData();
	}
	public void OnElvesUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.ELVES);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeSapientInfectionTime(RACE.ELVES);
		
		UpdateElvesInfectionTimeData();
	}
	public void OnHumansUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.HUMANS);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeSapientInfectionTime(RACE.HUMANS);
		
		UpdateHumansInfectionTimeData();
	}
	public void OnMonstersUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetMonsterInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeMonsterInfectionTime();
		
		UpdateMonstersInfectionTimeData();
	}
	public void OnUndeadUpgradeClicked() {
		int upgradeCost = PlagueDisease.Instance.lifespan.GetUndeadInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-upgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeUndeadInfectionTime();
		
		UpdateUndeadInfectionTimeData();
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
	#endregion
	
	private void ShowTooltip(string p_lifespanHeader, string p_lifespanDescription, UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null) { UIManager.Instance.ShowSmallInfo(p_lifespanDescription, p_hoverPosition, p_lifespanHeader); }
	}
	private void HideTooltip() {
		if (UIManager.Instance != null) { UIManager.Instance.HideSmallInfo(); }
	}

	private bool CanAffordUpgrade(int cost) {
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= cost;
		} else {
			return true;
		}
	}
	
	private void UpdateTileObjectInfectionTimeData() {
		m_lifeSpanUIView.UpdateTileObjectInfectionTime(GetInfectionTimeString(PlagueDisease.Instance.lifespan.tileObjectInfectionTimeInHours));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetTileObjectInfectionTimeUpgradeCost();
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsTileObjectAtMaxLevel();
		m_lifeSpanUIView.UpdateTileObjectUpgradePrice(nextUpgradeCost.ToString());
		m_lifeSpanUIView.UpdateTileObjectUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
		m_lifeSpanUIView.UpdateTileObjectUpgradePriceState(!isAtMaxLevel);
	}
	private void UpdateElvesInfectionTimeData() {
		m_lifeSpanUIView.UpdateElvesInfectionTime(GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlague(RACE.ELVES)));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.ELVES);
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsSapientAtMaxLevel(RACE.ELVES);
		m_lifeSpanUIView.UpdateElvesUpgradePrice(nextUpgradeCost.ToString());
		m_lifeSpanUIView.UpdateElvesUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
		m_lifeSpanUIView.UpdateElvesUpgradePriceState(!isAtMaxLevel);
	}
	private void UpdateHumansInfectionTimeData() {
		m_lifeSpanUIView.UpdateHumansInfectionTime(GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlague(RACE.HUMANS)));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.HUMANS);
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsSapientAtMaxLevel(RACE.HUMANS);
		m_lifeSpanUIView.UpdateHumansUpgradePrice(nextUpgradeCost.ToString());
		m_lifeSpanUIView.UpdateHumansUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
		m_lifeSpanUIView.UpdateHumansUpgradePriceState(!isAtMaxLevel);
	}
	private void UpdateMonstersInfectionTimeData() {
		m_lifeSpanUIView.UpdateMonstersInfectionTime(GetInfectionTimeString(PlagueDisease.Instance.lifespan.monsterInfectionTimeInHours));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetMonsterInfectionTimeUpgradeCost();
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsMonstersAtMaxLevel();
		m_lifeSpanUIView.UpdateMonstersUpgradePrice(nextUpgradeCost.ToString());
		m_lifeSpanUIView.UpdateMonstersUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
		m_lifeSpanUIView.UpdateMonstersUpgradePriceState(!isAtMaxLevel);
	}
	private void UpdateUndeadInfectionTimeData() {
		m_lifeSpanUIView.UpdateUndeadInfectionTime(GetInfectionTimeString(PlagueDisease.Instance.lifespan.undeadInfectionTimeInHours));
		int nextUpgradeCost = PlagueDisease.Instance.lifespan.GetUndeadInfectionTimeUpgradeCost();
		bool isAtMaxLevel = PlagueDisease.Instance.lifespan.IsUndeadAtMaxLevel();
		m_lifeSpanUIView.UpdateUndeadUpgradePrice(nextUpgradeCost.ToString());
		m_lifeSpanUIView.UpdateUndeadUpgradeButtonInteractable(!isAtMaxLevel && CanAffordUpgrade(nextUpgradeCost));
		m_lifeSpanUIView.UpdateUndeadUpgradePriceState(!isAtMaxLevel);
	}

	private string GetInfectionTimeString(int timeInHours) {
		if (timeInHours == 0) {
			return "Indefinite";
		} else if (timeInHours == -1) {
			return "Immune";
		}
		else {
			return $"{timeInHours.ToString()} Hours";
		}
	}
}