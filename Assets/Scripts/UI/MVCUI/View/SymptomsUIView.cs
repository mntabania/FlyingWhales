using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Ruinarch.Custom_UI;
using TMPro;
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
		void OnHoverOverParalysis(UIHoverPosition p_hoverPosition);
		void OnHoverOverVomiting(UIHoverPosition p_hoverPosition);
		void OnHoverOverLethargy(UIHoverPosition p_hoverPosition);
		void OnHoverOverSeizures(UIHoverPosition p_hoverPosition);
		void OnHoverOverInsomnia(UIHoverPosition p_hoverPosition);
		void OnHoverOverPoisonCloud(UIHoverPosition p_hoverPosition);
		void OnHoverOverMonsterScent(UIHoverPosition p_hoverPosition);
		void OnHoverOverSneezing(UIHoverPosition p_hoverPosition);
		void OnHoverOverDepression(UIHoverPosition p_hoverPosition);
		void OnHoverOverHungerPangs(UIHoverPosition p_hoverPosition);
		void OnHoverOutParalysis();
		void OnHoverOutVomiting();
		void OnHoverOutLethargy();
		void OnHoverOutSeizures();
		void OnHoverOutInsomnia();
		void OnHoverOutPoisonCloud();
		void OnHoverOutMonsterScent();
		void OnHoverOutSneezing();
		void OnHoverOutDepression();
		void OnHoverOutHungerPangs();
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
		UIModel.onParalysisHoveredOver += p_listener.OnHoverOverParalysis;
		UIModel.onVomitingHoveredOver += p_listener.OnHoverOverVomiting;
		UIModel.onLethargyHoveredOver += p_listener.OnHoverOverLethargy;
		UIModel.onSeizuresHoveredOver += p_listener.OnHoverOverSeizures;
		UIModel.onInsomniaHoveredOver += p_listener.OnHoverOverInsomnia;
		UIModel.onPoisonCloudHoveredOver += p_listener.OnHoverOverPoisonCloud;
		UIModel.onMonsterScentHoveredOver += p_listener.OnHoverOverMonsterScent;
		UIModel.onSneezingHoveredOver += p_listener.OnHoverOverSneezing;
		UIModel.onDepressionHoveredOver += p_listener.OnHoverOverDepression;
		UIModel.onHungerPangsHoveredOver += p_listener.OnHoverOverHungerPangs;
		UIModel.onParalysisHoveredOut += p_listener.OnHoverOutParalysis;
		UIModel.onVomitingHoveredOut += p_listener.OnHoverOutVomiting;
		UIModel.onLethargyHoveredOut += p_listener.OnHoverOutLethargy;
		UIModel.onSeizuresHoveredOut += p_listener.OnHoverOutSeizures;
		UIModel.onInsomniaHoveredOut += p_listener.OnHoverOutInsomnia;
		UIModel.onPoisonCloudHoveredOut += p_listener.OnHoverOutPoisonCloud;
		UIModel.onMonsterScentHoveredOut += p_listener.OnHoverOutMonsterScent;
		UIModel.onSneezingHoveredOut += p_listener.OnHoverOutSneezing;
		UIModel.onDepressionHoveredOut += p_listener.OnHoverOutDepression;
		UIModel.onHungerPangsHoveredOut += p_listener.OnHoverOutHungerPangs;
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
		UIModel.onParalysisHoveredOver -= p_listener.OnHoverOverParalysis;
		UIModel.onVomitingHoveredOver -= p_listener.OnHoverOverVomiting;
		UIModel.onLethargyHoveredOver -= p_listener.OnHoverOverLethargy;
		UIModel.onSeizuresHoveredOver -= p_listener.OnHoverOverSeizures;
		UIModel.onInsomniaHoveredOver -= p_listener.OnHoverOverInsomnia;
		UIModel.onPoisonCloudHoveredOver -= p_listener.OnHoverOverPoisonCloud;
		UIModel.onMonsterScentHoveredOver -= p_listener.OnHoverOverMonsterScent;
		UIModel.onSneezingHoveredOver -= p_listener.OnHoverOverSneezing;
		UIModel.onDepressionHoveredOver -= p_listener.OnHoverOverDepression;
		UIModel.onHungerPangsHoveredOver -= p_listener.OnHoverOverHungerPangs;
		UIModel.onParalysisHoveredOut -= p_listener.OnHoverOutParalysis;
		UIModel.onVomitingHoveredOut -= p_listener.OnHoverOutVomiting;
		UIModel.onLethargyHoveredOut -= p_listener.OnHoverOutLethargy;
		UIModel.onSeizuresHoveredOut -= p_listener.OnHoverOutSeizures;
		UIModel.onInsomniaHoveredOut -= p_listener.OnHoverOutInsomnia;
		UIModel.onPoisonCloudHoveredOut -= p_listener.OnHoverOutPoisonCloud;
		UIModel.onMonsterScentHoveredOut -= p_listener.OnHoverOutMonsterScent;
		UIModel.onSneezingHoveredOut -= p_listener.OnHoverOutSneezing;
		UIModel.onDepressionHoveredOut -= p_listener.OnHoverOutDepression;
		UIModel.onHungerPangsHoveredOut -= p_listener.OnHoverOutHungerPangs;
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
	private TextMeshProUGUI GetSymptomUpgradeBtnText(PLAGUE_SYMPTOM p_symptomType) {
		switch (p_symptomType) {
			case PLAGUE_SYMPTOM.Paralysis:
				return UIModel.txtParalysisUpgrade;
			case PLAGUE_SYMPTOM.Vomiting:
				return UIModel.txtVomitingUpgrade;
			case PLAGUE_SYMPTOM.Lethargy:
				return UIModel.txtLethargyUpgrade;
			case PLAGUE_SYMPTOM.Seizure:
				return UIModel.txtSeizuresUpgrade;
			case PLAGUE_SYMPTOM.Insomnia:
				return UIModel.txtInsomniaUpgrade;
			case PLAGUE_SYMPTOM.Poison_Cloud:
				return UIModel.txtPoisonCloudUpgrade;
			case PLAGUE_SYMPTOM.Monster_Scent:
				return UIModel.txtMonsterScenetUpgrade;
			case PLAGUE_SYMPTOM.Sneezing:
				return UIModel.txtSneezingUpgrade;
			case PLAGUE_SYMPTOM.Depression:
				return UIModel.txtDepressionUpgrade;
			case PLAGUE_SYMPTOM.Hunger_Pangs:
				return UIModel.txtHungerUpgrade;
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
		TextMeshProUGUI text = GetSymptomUpgradeBtnText(p_symptom);
		text.color = UtilityScripts.GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
}