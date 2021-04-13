using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Plague.Death_Effect;
using UnityEngine.Assertions;

public class OnDeathUIController : MVCUIController, OnDeathUIView.IListener
{
	[SerializeField]
	private OnDeathUIModel m_onDeathUIModel;
	private OnDeathUIView m_onDeathUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		OnDeathUIView.Create(_canvas, m_onDeathUIModel, (p_ui) => {
			m_onDeathUIView = p_ui;
			m_onDeathUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}
	public override void ShowUI() {
		base.ShowUI();
		UpdateAllDeathEffects();
	}
	private void UpdateAllDeathEffects() {
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Explosion);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Zombie);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Chaos_Generator);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
	}

	private void UpdateDeathEffectData(PLAGUE_DEATH_EFFECT p_deathEffect) {
		if (PlagueDisease.Instance.HasMaxDeathEffect() && PlagueDisease.Instance.IsDeathEffectActive(p_deathEffect, out var deathEffect)) {
			int upgradeCost = deathEffect.GetFinalNextLevelUpgradeCost();
			bool isMaxLevel = upgradeCost == -1;
			m_onDeathUIView.UpdateDeathEffectCost(p_deathEffect, isMaxLevel ? "MAX" : $"{upgradeCost.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
			m_onDeathUIView.UpdateDeathEffectDescription(p_deathEffect, deathEffect.GetCurrentEffectDescription());
			m_onDeathUIView.UpdateDeathEffectUpgradeButtonInteractable(p_deathEffect,  !isMaxLevel && CanAffordUnlockOrUpgrade(p_deathEffect));
		} else {
			m_onDeathUIView.UpdateDeathEffectCost(p_deathEffect, $"{p_deathEffect.GetUnlockCost().ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()}");
			m_onDeathUIView.UpdateDeathEffectDescription(p_deathEffect, string.Empty);
			m_onDeathUIView.UpdateDeathEffectUpgradeButtonInteractable(p_deathEffect, !PlagueDisease.Instance.HasMaxDeathEffect() && CanAffordUnlockOrUpgrade(p_deathEffect));
		}
	}
	
	#region OnDeathUIView.IListener implementation
	public void OnIgniteUpgradeClicked() {
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Explosion);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Explosion);
		UpdateAllDeathEffects();
	}
	public void OnWalkerZombieUpgradeClicked() {
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Zombie);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Zombie);
		UpdateAllDeathEffects();
	}
	public void OnMana2_3UpgradeClicked() {
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Chaos_Generator);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Chaos_Generator);
		UpdateAllDeathEffects();
	}
	public void OnRandomSpirit_1UpgradeClicked() {
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
		UpdateAllDeathEffects();
	}
	public void OnIgniteHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_DEATH_EFFECT.Explosion, hoverPosition); }
	public void OnWalkerZombieHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_DEATH_EFFECT.Zombie, hoverPosition); }
	public void OnManaHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_DEATH_EFFECT.Chaos_Generator, hoverPosition); }
	public void OnSpiritHoveredOver(UIHoverPosition hoverPosition) { ShowTooltip(PLAGUE_DEATH_EFFECT.Haunted_Spirits, hoverPosition); }
	public void OnIgniteHoveredOut() { HideTooltip(); }
	public void OnWalkerZombieHoveredOut() { HideTooltip(); }
	public void OnManaHoveredOut() { HideTooltip(); }
	public void OnSpiritHoveredOut() { HideTooltip(); }
	#endregion
	
	private void ShowTooltip(PLAGUE_DEATH_EFFECT p_deathEffectType, UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null) {
			int currentLevel = 0;
			if (PlagueDisease.Instance.IsDeathEffectActive(p_deathEffectType, out var deathEffect)) {
				currentLevel = deathEffect.level;
			}
			string summary = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>Current Effect:</font>";
			summary = $"{summary}\n<line-height=70%><size=16>{p_deathEffectType.GetEffectTooltip(currentLevel)}";
			summary = $"{summary}\n\n<color=\"green\"><font=\"Eczar-Medium\"><line-height=100%><size=18>On Upgrade:</font>";
			summary = $"{summary}\n<line-height=70%><size=16>{p_deathEffectType.GetEffectTooltip(currentLevel + 1)}";
			
			UIManager.Instance.ShowSmallInfo(summary, p_hoverPosition);
		}
	}
	private void HideTooltip() {
		if (UIManager.Instance != null) { UIManager.Instance.HideSmallInfo(); }
	}

	private void SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT p_deathEffect) {
		if (PlagueDisease.Instance.activeDeathEffect != null) {
			Assert.IsTrue(PlagueDisease.Instance.activeDeathEffect.deathEffectType == p_deathEffect);
			//upgrade
			PlagueDisease.Instance.activeDeathEffect.AdjustLevel(1);
		} else {
			//unlock
			PlagueDisease.Instance.SetNewPlagueDeathEffectAndUnsetPrev(p_deathEffect);
		}
	}
	private void PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT p_deathEffect) {
		var cost = GetUnlockOrUpgradeCost(p_deathEffect);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-cost);
		}
	}
	private bool CanAffordUnlockOrUpgrade(PLAGUE_DEATH_EFFECT p_deathEffect) {
		var cost = GetUnlockOrUpgradeCost(p_deathEffect);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			return PlayerManager.Instance.player.plagueComponent.plaguePoints >= cost || (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None);
		} else {
			return true;	
		}
	}
	private int GetUnlockOrUpgradeCost(PLAGUE_DEATH_EFFECT p_deathEffect) {
		var cost = PlagueDisease.Instance.activeDeathEffect != null ? PlagueDisease.Instance.activeDeathEffect.GetFinalNextLevelUpgradeCost() : p_deathEffect.GetUnlockCost();
		return cost;
	}
}