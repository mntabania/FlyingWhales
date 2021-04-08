using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using TMPro;
using UnityEngine.UI;

public class OnDeathUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnIgniteUpgradeClicked();
		void OnWalkerZombieUpgradeClicked();
		void OnMana2_3UpgradeClicked();
		void OnRandomSpirit_1UpgradeClicked();
		void OnIgniteHoveredOver(UIHoverPosition hoverPosition);
		void OnWalkerZombieHoveredOver(UIHoverPosition hoverPosition);
		void OnManaHoveredOver(UIHoverPosition hoverPosition);
		void OnSpiritHoveredOver(UIHoverPosition hoverPosition);
		void OnIgniteHoveredOut();
		void OnWalkerZombieHoveredOut();
		void OnManaHoveredOut();
		void OnSpiritHoveredOut();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public OnDeathUIModel UIModel
	{
		get
		{
			return _baseAssetModel as OnDeathUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, OnDeathUIModel p_assets, Action<OnDeathUIView> p_onCreate)
	{
		var go = new GameObject(typeof(OnDeathUIView).ToString());
		var gui = go.AddComponent<OnDeathUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.onIgniteUpgradeClicked += p_listener.OnIgniteUpgradeClicked;
		UIModel.onWalkerZombieUpgradeClicked += p_listener.OnWalkerZombieUpgradeClicked;
		UIModel.onMana2_3UpgradeClicked += p_listener.OnMana2_3UpgradeClicked;
		UIModel.onRandomSpirit_1UpgradeClicked += p_listener.OnRandomSpirit_1UpgradeClicked;
		UIModel.onIgniteHoveredOver += p_listener.OnIgniteHoveredOver;
		UIModel.onWalkerZombieHoveredOver += p_listener.OnWalkerZombieHoveredOver;
		UIModel.onManaHoveredOver += p_listener.OnManaHoveredOver;
		UIModel.onSpiritHoveredOver += p_listener.OnSpiritHoveredOver;
		UIModel.onIgniteHoveredOut += p_listener.OnIgniteHoveredOut;
		UIModel.onWalkerZombieHoveredOut += p_listener.OnWalkerZombieHoveredOut;
		UIModel.onManaHoveredOut += p_listener.OnManaHoveredOut;
		UIModel.onSpiritHoveredOut += p_listener.OnSpiritHoveredOut;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onIgniteUpgradeClicked -= p_listener.OnIgniteUpgradeClicked;
		UIModel.onWalkerZombieUpgradeClicked -= p_listener.OnWalkerZombieUpgradeClicked;
		UIModel.onMana2_3UpgradeClicked -= p_listener.OnMana2_3UpgradeClicked;
		UIModel.onRandomSpirit_1UpgradeClicked -= p_listener.OnRandomSpirit_1UpgradeClicked;
		UIModel.onIgniteHoveredOver -= p_listener.OnIgniteHoveredOver;
		UIModel.onWalkerZombieHoveredOver -= p_listener.OnWalkerZombieHoveredOver;
		UIModel.onManaHoveredOver -= p_listener.OnManaHoveredOver;
		UIModel.onSpiritHoveredOver -= p_listener.OnSpiritHoveredOver;
		UIModel.onIgniteHoveredOut -= p_listener.OnIgniteHoveredOut;
		UIModel.onWalkerZombieHoveredOut -= p_listener.OnWalkerZombieHoveredOut;
		UIModel.onManaHoveredOut -= p_listener.OnManaHoveredOut;
		UIModel.onSpiritHoveredOut -= p_listener.OnSpiritHoveredOut;
	}
	#endregion

	private RuinarchText GetCostTextToUpdate(PLAGUE_DEATH_EFFECT p_deathEffect) {
		switch (p_deathEffect) {
			case PLAGUE_DEATH_EFFECT.Explosion:
				return UIModel.txtIgniteUpgradeCost;
			case PLAGUE_DEATH_EFFECT.Zombie:
				return UIModel.txtWalkerZombieUpgradeCost;
			case PLAGUE_DEATH_EFFECT.Chaos_Generator:
				return UIModel.txtMana2_3UpgradeCost;
			case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
				return UIModel.txtRandomSpirit_1UpgradeCost;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
		}
	}
	private RuinarchText GetEffectTextToUpdate(PLAGUE_DEATH_EFFECT p_deathEffect) {
		switch (p_deathEffect) {
			case PLAGUE_DEATH_EFFECT.Explosion:
				return UIModel.txtIgniteEffect;
			case PLAGUE_DEATH_EFFECT.Zombie:
				return UIModel.txtWalkerZombieEffect;
			case PLAGUE_DEATH_EFFECT.Chaos_Generator:
				return UIModel.txtMana2_3Effect;
			case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
				return UIModel.txtRandomSpirit_1Effect;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
		}
	}
	private Button GetDeathEffectUpgradeButton(PLAGUE_DEATH_EFFECT p_deathEffect) {
		switch (p_deathEffect) {
			case PLAGUE_DEATH_EFFECT.Explosion:
				return UIModel.btnIgniteUpgrade;
			case PLAGUE_DEATH_EFFECT.Zombie:
				return UIModel.btnWalkerZombieUpgrade;
			case PLAGUE_DEATH_EFFECT.Chaos_Generator:
				return UIModel.btnMana2_3Upgrade;
			case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
				return UIModel.btnRandomSpirit_1Upgrade;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
		}
	}
	private TextMeshProUGUI GetDeathEffectUpgradeText(PLAGUE_DEATH_EFFECT p_deathEffect) {
		switch (p_deathEffect) {
			case PLAGUE_DEATH_EFFECT.Explosion:
				return UIModel.txtIgniteUpgrade;
			case PLAGUE_DEATH_EFFECT.Zombie:
				return UIModel.txtWalkerZombieUpgrade;
			case PLAGUE_DEATH_EFFECT.Chaos_Generator:
				return UIModel.txtMana2_3Upgrade;
			case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
				return UIModel.txtRandomSpirit_1Upgrade;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
		}
	}
	
	public void UpdateDeathEffectCost(PLAGUE_DEATH_EFFECT p_deathEffect, string p_cost) {
		RuinarchText txt = GetCostTextToUpdate(p_deathEffect);
		txt.text = p_cost;
	}
	public void UpdateDeathEffectUpgradeButtonInteractable(PLAGUE_DEATH_EFFECT p_deathEffect, bool p_interactable) {
		Button button = GetDeathEffectUpgradeButton(p_deathEffect);
		button.interactable = p_interactable;
		TextMeshProUGUI text = GetDeathEffectUpgradeText(p_deathEffect);
		text.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
	public void UpdateDeathEffectDescription(PLAGUE_DEATH_EFFECT p_deathEffect, string p_effect) {
		RuinarchText txt = GetEffectTextToUpdate(p_deathEffect);
		txt.text = p_effect;
	}
}