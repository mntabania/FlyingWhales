using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using UnityEngine.UI;

public class SymptomsUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnParalysisUpgradeClicked();
		void OnVomitingUpgradeClicked();
		void OnLethargyUpgradeClicked();
		void OnSeizuresUpgradeClicked();
		void OnInsomniaUpgradeClicked();
		void OnPoisonCloudUpgradeClicked();
		void OnMonsterScentUpgradeClicked();
		void OnSneezingUpgradeClicked();
		void OnDepressionUpgradeClicked();
		void OnHungerPangsUpgradeClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public SymptomsUIModel UIModel
	{
		get
		{
			return _baseAssetModel as SymptomsUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, SymptomsUIModel p_assets, Action<SymptomsUIView> p_onCreate)
	{
		var go = new GameObject(typeof(SymptomsUIView).ToString());
		var gui = go.AddComponent<SymptomsUIView>();
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
		UIModel.onParalysisUpgradeClicked += p_listener.OnParalysisUpgradeClicked;
		UIModel.onVomitingUpgradeClicked += p_listener.OnVomitingUpgradeClicked;
		UIModel.onLethargyUpgradeClicked += p_listener.OnLethargyUpgradeClicked;
		UIModel.onSeizuresUpgradeClicked += p_listener.OnSeizuresUpgradeClicked;
		UIModel.onInsomniaUpgradeClicked += p_listener.OnInsomniaUpgradeClicked;
		UIModel.onPoisonCloudUpgradeClicked += p_listener.OnPoisonCloudUpgradeClicked;
		UIModel.onMonsterScentUpgradeClicked += p_listener.OnMonsterScentUpgradeClicked;
		UIModel.onSneezingUpgradeClicked += p_listener.OnSneezingUpgradeClicked;
		UIModel.onDepressionUpgradeClicked += p_listener.OnDepressionUpgradeClicked;
		UIModel.onHungerPangsUpgradeClicked += p_listener.OnHungerPangsUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onParalysisUpgradeClicked -= p_listener.OnParalysisUpgradeClicked;
		UIModel.onVomitingUpgradeClicked -= p_listener.OnVomitingUpgradeClicked;
		UIModel.onLethargyUpgradeClicked -= p_listener.OnLethargyUpgradeClicked;
		UIModel.onSeizuresUpgradeClicked -= p_listener.OnSeizuresUpgradeClicked;
		UIModel.onInsomniaUpgradeClicked -= p_listener.OnInsomniaUpgradeClicked;
		UIModel.onPoisonCloudUpgradeClicked -= p_listener.OnPoisonCloudUpgradeClicked;
		UIModel.onMonsterScentUpgradeClicked -= p_listener.OnMonsterScentUpgradeClicked;
		UIModel.onSneezingUpgradeClicked -= p_listener.OnSneezingUpgradeClicked;
		UIModel.onDepressionUpgradeClicked -= p_listener.OnDepressionUpgradeClicked;
		UIModel.onHungerPangsUpgradeClicked -= p_listener.OnHungerPangsUpgradeClicked;
	}
	#endregion

	private RuinarchText GetCostTextToUpdate(PLAGUE_SYMPTOM p_symptomType) {
		switch (p_symptomType) {
			case PLAGUE_SYMPTOM.Paralysis:
				return UIModel.txtParalysisCost;
			case PLAGUE_SYMPTOM.Vomiting:
				return UIModel.txtVomitingCost;
			case PLAGUE_SYMPTOM.Lethargy:
				return UIModel.txtLethargyCost;
			case PLAGUE_SYMPTOM.Seizure:
				return UIModel.txtSeizuresCost;
			case PLAGUE_SYMPTOM.Insomnia:
				return UIModel.txtInsomniaCost;
			case PLAGUE_SYMPTOM.Poison_Cloud:
				return UIModel.txtPoisonCloudCost;
			case PLAGUE_SYMPTOM.Monster_Scent:
				return UIModel.txtMonsterScentCost;
			case PLAGUE_SYMPTOM.Sneezing:
				return UIModel.txtSneezingCost;
			case PLAGUE_SYMPTOM.Depression:
				return UIModel.txtDepressionCost;
			case PLAGUE_SYMPTOM.Hunger_Pangs:
				return UIModel.txtHungerCost;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_symptomType), p_symptomType, null);
		}
	}
	private Button GetSymptomUpgradeBtn(PLAGUE_SYMPTOM p_symptomType) {
		switch (p_symptomType) {
			case PLAGUE_SYMPTOM.Paralysis:
				return UIModel.btnParalysisUpgrade;
			case PLAGUE_SYMPTOM.Vomiting:
				return UIModel.btnVomitingUpgrade;
			case PLAGUE_SYMPTOM.Lethargy:
				return UIModel.btnLethargyUpgrade;
			case PLAGUE_SYMPTOM.Seizure:
				return UIModel.btnSeizuresUpgrade;
			case PLAGUE_SYMPTOM.Insomnia:
				return UIModel.btnInsomniaUpgrade;
			case PLAGUE_SYMPTOM.Poison_Cloud:
				return UIModel.btnPoisonCloudUpgrade;
			case PLAGUE_SYMPTOM.Monster_Scent:
				return UIModel.btnMonsterScenetUpgrade;
			case PLAGUE_SYMPTOM.Sneezing:
				return UIModel.btnSneezingUpgrade;
			case PLAGUE_SYMPTOM.Depression:
				return UIModel.btnDepressionUpgrade;
			case PLAGUE_SYMPTOM.Hunger_Pangs:
				return UIModel.btnHungerUpgrade;
			default:
				throw new ArgumentOutOfRangeException(nameof(p_symptomType), p_symptomType, null);
		}
	}

	public void UpdateSymptomCost(PLAGUE_SYMPTOM p_symptom, string p_cost) {
		RuinarchText txt = GetCostTextToUpdate(p_symptom);
		txt.text = p_cost;
	}
	public void UpdateSymptomCostState(PLAGUE_SYMPTOM p_symptom, bool p_state) {
		RuinarchText txt = GetCostTextToUpdate(p_symptom);
		txt.gameObject.SetActive(p_state);
	}
	public void UpdateSymptomUpgradeButtonInteractable(PLAGUE_SYMPTOM p_symptom, bool p_interactable) {
		Button button = GetSymptomUpgradeBtn(p_symptom);
		button.interactable = p_interactable;
	}
}