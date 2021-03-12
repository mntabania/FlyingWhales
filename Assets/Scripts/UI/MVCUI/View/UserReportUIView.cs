using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using UnityEngine.UI;

public class UserReportUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnSubmitClicked();
		void OnCloseClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public UserReportUIModel UIModel {
		get {
			return _baseAssetModel as UserReportUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, UserReportUIModel p_assets, Action<UserReportUIView> p_onCreate) {
		var go = new GameObject(typeof(UserReportUIView).ToString());
		var gui = go.AddComponent<UserReportUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null) {
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	public UserReportingScript GetUserReportingScript() {
		return UIModel.userReportingScript;
	}

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onSubmitClicked += p_listener.OnSubmitClicked;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onSubmitClicked -= p_listener.OnSubmitClicked;
	}
	#endregion
}