using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class SkillUpgradeUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnAfflictionTabClicked(bool isOn);
		void OnSpellTabClicked(bool isOn);
		void OnPlayerActionTabClicked(bool isOn);
		void OnCloseClicked();
		//void OnHoveredOverPlaguedRat(UIHoverPosition p_hoverPosition);
		//void OnHoveredOutPlaguedRat();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public SkillUpgradeUIModel UIModel {
		get {
			return _baseAssetModel as SkillUpgradeUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, SkillUpgradeUIModel p_assets, Action<SkillUpgradeUIView> p_onCreate) {
		var go = new GameObject(typeof(SkillUpgradeUIView).ToString());
		var gui = go.AddComponent<SkillUpgradeUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null) {
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region user defined functions

	public Transform GetSkillParent() {
		return UIModel.skillParent;
	}
	public Transform GetTabParentTransform() {
		return UIModel.tabPrent;
	}
	public void SetUnlockSkillCount(string p_activeCasesCount) {
		UIModel.txtTotalUnlocked.text = p_activeCasesCount;
	}
	public void SetChaticEnergyCount(string p_deathCount) {
		UIModel.txtChaoticEnergyAmount.text = p_deathCount;
	}
	
	public void SetTransmissionTabIsOnWithoutNotify(bool p_isOn) {
		UIModel.btnAfflictionTab.SetIsOnWithoutNotify(p_isOn);
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onAfflictionTabClicked += p_listener.OnAfflictionTabClicked;
		UIModel.onSpellTabClicked += p_listener.OnSpellTabClicked;
		UIModel.onPlayerActionTabClicked += p_listener.OnPlayerActionTabClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		//UIModel.onPlaguedRatsHoveredOver += p_listener.OnHoveredOverPlaguedRat;
		//UIModel.onPlaguedRatsHoveredOut += p_listener.OnHoveredOutPlaguedRat;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onAfflictionTabClicked -= p_listener.OnAfflictionTabClicked;
		UIModel.onSpellTabClicked -= p_listener.OnSpellTabClicked;
		UIModel.onPlayerActionTabClicked -= p_listener.OnPlayerActionTabClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		//UIModel.onPlaguedRatsHoveredOver -= p_listener.OnHoveredOverPlaguedRat;
		//UIModel.onPlaguedRatsHoveredOut -= p_listener.OnHoveredOutPlaguedRat;
	}
	#endregion
}