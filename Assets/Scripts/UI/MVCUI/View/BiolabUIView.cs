using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class BiolabUIView : MVCUIView
{
	#region interface for listener
	public interface IListener
	{
		void OnTransmissionTabClicked(bool isOn);
		void OnLifeSpanTabClicked(bool isOn);
		void OnFatalityTabClicked(bool isOn);
		void OnSymptomsTabClicked(bool isOn);
		void OnOnDeathClicked(bool isOn);
		void OnCloseClicked();
		//void OnHoveredOverPlaguedRat(UIHoverPosition p_hoverPosition);
		//void OnHoveredOutPlaguedRat();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public BiolabUIModel UIModel
	{
		get
		{
			return _baseAssetModel as BiolabUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, BiolabUIModel p_assets, Action<BiolabUIView> p_onCreate)
	{
		var go = new GameObject(typeof(BiolabUIView).ToString());
		var gui = go.AddComponent<BiolabUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region user defined functions
	public Transform GetTabParentTransform() {
		return UIModel.tabPrent;
	}
	public void SetActiveCases(string p_activeCasesCount) {
		UIModel.txtActiveCasesValue.text = p_activeCasesCount;
	}
	public void SetDeathCases(string p_deathCount)
	{
		UIModel.txtDeathsValue.text = p_deathCount;
	}
	public void SetRecoveriesCases(string p_recoveriesCount)
	{
		UIModel.txtRecoveriesValue.text = p_recoveriesCount;
	}
	public void SetPlagueRats(string p_plagueRatsCount)
	{
		UIModel.txtPlagueRatsValue.text = p_plagueRatsCount;
	}
	public void SetPlaguePoints(string p_plaguePoints)
	{
		UIModel.txtPlaguePoints.text = p_plaguePoints;
	}
	public void SetTransmissionTabIsOnWithoutNotify(bool p_isOn) {
		UIModel.btnTransmissionTab.SetIsOnWithoutNotify(p_isOn);
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.onTransmissionTabClicked += p_listener.OnTransmissionTabClicked;
		UIModel.onLifeSpanTabClicked += p_listener.OnLifeSpanTabClicked;
		UIModel.onFatalityTabClicked += p_listener.OnFatalityTabClicked;
		UIModel.onSymptomsTabClicked += p_listener.OnSymptomsTabClicked;
		UIModel.onOnDeathClicked += p_listener.OnOnDeathClicked;
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		//UIModel.onPlaguedRatsHoveredOver += p_listener.OnHoveredOverPlaguedRat;
		//UIModel.onPlaguedRatsHoveredOut += p_listener.OnHoveredOutPlaguedRat;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onTransmissionTabClicked -= p_listener.OnTransmissionTabClicked;
		UIModel.onLifeSpanTabClicked -= p_listener.OnLifeSpanTabClicked;
		UIModel.onFatalityTabClicked -= p_listener.OnFatalityTabClicked;
		UIModel.onSymptomsTabClicked -= p_listener.OnSymptomsTabClicked;
		UIModel.onOnDeathClicked -= p_listener.OnOnDeathClicked;
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		//UIModel.onPlaguedRatsHoveredOver -= p_listener.OnHoveredOverPlaguedRat;
		//UIModel.onPlaguedRatsHoveredOut -= p_listener.OnHoveredOutPlaguedRat;
	}
	#endregion
}