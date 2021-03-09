using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class PurchaseSkillUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnRerollClicked();
		void OnCloseClicked();
		void OnHoverOverReroll();
		void OnHoverOutReroll();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public PurchaseSkillUIModel UIModel {
		get {
			return _baseAssetModel as PurchaseSkillUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, PurchaseSkillUIModel p_assets, Action<PurchaseSkillUIView> p_onCreate) {
		var go = new GameObject(typeof(PurchaseSkillUIView).ToString());
		var gui = go.AddComponent<PurchaseSkillUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null) {
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region user defined functions
	public void DisableRerollButton() {
		UIModel.btnReroll.interactable = false;
	}
	public void EnableRerollButton() {
		UIModel.btnReroll.interactable = true;
	}

	public void ShowSkills() {
		UIModel.skillsParent.gameObject.SetActive(true);
		UIModel.txtMessageDisplay.gameObject.SetActive(false);
	}
	public void HideSkills() {
		UIModel.skillsParent.gameObject.SetActive(false);
		UIModel.txtMessageDisplay.gameObject.SetActive(true);
	}

	public Transform GetSkillsParent() {
		return UIModel.skillsParent;
	}

	public void SetMessage(string p_message) {
		UIModel.txtMessageDisplay.text = p_message;
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onRerollClicked += p_listener.OnRerollClicked;
		UIModel.onHoverOverReroll += p_listener.OnHoverOverReroll;
		UIModel.onHoverOutReroll += p_listener.OnHoverOutReroll;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onRerollClicked -= p_listener.OnRerollClicked;
		UIModel.onHoverOverReroll -= p_listener.OnHoverOverReroll;
		UIModel.onHoverOutReroll -= p_listener.OnHoverOutReroll;
	}
	#endregion
}