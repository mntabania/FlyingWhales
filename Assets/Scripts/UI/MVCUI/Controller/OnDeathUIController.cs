﻿using UnityEngine;
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
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Mana_Generator);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
	}

	private void UpdateDeathEffectData(PLAGUE_DEATH_EFFECT p_deathEffect) {
		if (PlagueDisease.Instance.HasMaxDeathEffect() && PlagueDisease.Instance.IsDeathEffectActive(p_deathEffect, out var deathEffect)) {
			int upgradeCost = deathEffect.GetNextLevelUpgradeCost();
			m_onDeathUIView.UpdateDeathEffectCost(p_deathEffect, upgradeCost.ToString());
			m_onDeathUIView.UpdateDeathEffectDescription(p_deathEffect, deathEffect.GetCurrentEffectDescription());
			m_onDeathUIView.UpdateDeathEffectUpgradeButtonInteractable(p_deathEffect,  upgradeCost != -1 && CanAffordUnlockOrUpgrade(p_deathEffect));
			m_onDeathUIView.UpdateDeathEffectCostState(p_deathEffect, upgradeCost != -1);
		} else {
			m_onDeathUIView.UpdateDeathEffectCost(p_deathEffect, p_deathEffect.GetUnlockCost().ToString());
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
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Mana_Generator);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Mana_Generator);
		UpdateAllDeathEffects();
	}
	public void OnRandomSpirit_1UpgradeClicked() {
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
		UpdateAllDeathEffects();
	}
	public void OnIgniteHoveredOver(UIHoverPosition hoverPosition) { }
	public void OnWalkerZombieHoveredOver(UIHoverPosition hoverPosition) { }
	public void OnManaHoveredOver(UIHoverPosition hoverPosition) { }
	public void OnSpiritHoveredOver(UIHoverPosition hoverPosition) { }
	public void OnIgniteHoveredOut() { }
	public void OnWalkerZombieHoveredOut() { }
	public void OnManaHoveredOut() { }
	public void OnSpiritHoveredOut() { }
	#endregion

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
			return PlayerManager.Instance.player.plagueComponent.plaguePoints > cost;
		} else {
			return true;	
		}
	}
	private int GetUnlockOrUpgradeCost(PLAGUE_DEATH_EFFECT p_deathEffect) {
		int cost;
		if (PlagueDisease.Instance.activeDeathEffect != null) {
			cost = PlagueDisease.Instance.activeDeathEffect.GetNextLevelUpgradeCost();
		}
		else {
			cost = p_deathEffect.GetUnlockCost();
		}
		return cost;
	}
}