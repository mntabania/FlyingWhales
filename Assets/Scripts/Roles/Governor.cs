﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Governor : Role {

	public City ownedCity;
	private int _loyalty;
    private const int defaultLoyalty = 0;
    private int _eventLoyaltyModifier;
    public int forTestingLoyaltyModifier;

    private string _loyaltySummary; //For UI, to display the factors that affected this governor's loyalty
    internal string _eventLoyaltySummary;

	private List<ExpirableModifier> _eventModifiers;

	private bool isInitial;

    #region getters/setters
    public int loyalty {
		get { return (_loyalty + _eventLoyaltyModifier + forTestingLoyaltyModifier); }
    }
    public string loyaltySummary {
        get { return this._loyaltySummary + _eventLoyaltySummary; }
    }
	public List<ExpirableModifier> eventModifiers {
		get { return this._eventModifiers; }
	}
    #endregion

    public Governor(Citizen citizen): base(citizen){
		this.citizen.city.governor = this.citizen;
		this.citizen.isGovernor = true;
		this.citizen.isKing = false;
        this._loyaltySummary = string.Empty;
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;
		this._eventModifiers = new List<ExpirableModifier> ();
		this.isInitial = true;

        this.SetOwnedCity(this.citizen.city);
        //this.citizen.GenerateCharacterValues();

		//PrestigeContribution (false);
		//StabilityContribution (false);
		//IntelligenceContribution (false);

        this.UpdateLoyalty ();
//		Messenger.AddListener("OnDayEnd", CheckEventModifiers);

	}
	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
	internal void AdjustLoyalty(int amount){
		this._loyalty += amount;
        this._loyalty = Mathf.Clamp(this._loyalty, -100, 100);

//		GovernorEvents ();
	}
//	private void PrestigeContribution(bool isRemove){
//		int contribution = 0;
//		switch(this.citizen.charisma){
//		case CHARISMA.CHARISMATIC:
//			contribution = 2;
//			break;
//		case CHARISMA.NEUTRAL:
//			contribution = 1;
//			break;
////		case CHARISMA.REPULSIVE:
////			contribution = 0;
////			break;
//		}
//		if(isRemove){
//			contribution *= -1;
//		}
//		if (contribution != 0) {
//			this.citizen.city.kingdom.AdjustBonusPrestige (contribution);
//		}

//	}
//	private void StabilityContribution(bool isRemove){
//		int contribution = 0;
//		switch(this.citizen.efficiency){
//		case EFFICIENCY.EFFICIENT:
//			contribution = 2;
//			break;
//		case EFFICIENCY.NEUTRAL:
//			contribution = 1;
//			break;
////		case EFFICIENCY.INEPT:
////			contribution = 0;
////			break;
//		}
//		if(isRemove){
//			contribution *= -1;
//		}
//		if(contribution != 0){
//			this.citizen.city.AdjustBonusStability (contribution);
//		}
//	}
//	private void IntelligenceContribution(bool isRemove){
//		int contribution = 0;
//		switch(this.citizen.intelligence){
//		case INTELLIGENCE.SMART:
//			contribution = 2;
//			break;
//		case INTELLIGENCE.NEUTRAL:
//			contribution = 1;
//			break;
////		case INTELLIGENCE.DUMB:
////			contribution = 0;
////			break;
//		}
//		if(isRemove){
//			contribution *= -1;
//		}
//		if (contribution != 0) {
//			this.citizen.city.kingdom.AdjustBonusTech (contribution);
//		}
//	}
	internal void UpdateLoyalty(){
        this._loyaltySummary = string.Empty;
		int baseLoyalty = defaultLoyalty;
		int adjustment = 0;
        //this._loyaltySummary += "+" + defaultLoyalty.ToString() + "   Base value\n";

        Citizen king = this.citizen.city.kingdom.king;
		Kingdom kingdom = this.citizen.city.kingdom;

        //List<CHARACTER_VALUE> governorValues = this.citizen.importantCharacterValues.Select(x => x.Key).ToList();
        //List<CHARACTER_VALUE> kingValues = king.importantCharacterValues.Select(x => x.Key).ToList();

        //List<CHARACTER_VALUE> valuesInCommon = governorValues.Intersect(kingValues).ToList();

  //      /*POSITIVE ADJUSTMENT OF LOYALTY
		// * */
  //      if (valuesInCommon.Count == 1) {
  //          adjustment = 0;
  //          baseLoyalty += adjustment;
  //          this._loyaltySummary += "+" + adjustment.ToString() + "   shared values.\n";
  //      } else if (valuesInCommon.Count == 2) {
  //          adjustment = 15;
  //          baseLoyalty += adjustment;
  //          this._loyaltySummary += "+" + adjustment.ToString() + "   shared values.\n";
  //      } else if(valuesInCommon.Count >= 3) {
  //         adjustment = 30;
  //          baseLoyalty += adjustment;
  //          this._loyaltySummary += "+" + adjustment.ToString() + "   shared values.\n";
		//} else{
		//	adjustment = -30;
		//	baseLoyalty += adjustment;
		//	this._loyaltySummary += adjustment.ToString() + "   no shared values.\n";
		//}

        //if (governorValues.Contains(CHARACTER_VALUE.HONOR)) {
        //    adjustment = 15;
        //    baseLoyalty += adjustment;
        //    this._loyaltySummary += "+" + adjustment.ToString() + "   values honor.\n";
        //}

        if (king.IsRelative(this.citizen)) {
            adjustment = 25;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   Governor is Relative of King.\n";
        }

        if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility >= 0){
            adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   King is husband of a relative and their compatibility is positive.\n";
        }

        /*NEGATIVE ADJUSTMENT OF LOYALTY
		 * */
//        if (!governorValues.Contains(CHARACTER_VALUE.HONOR)) {
//            int adjustment = -15;
//            baseLoyalty += adjustment;
//            this._loyaltySummary += adjustment.ToString() + "   does not value honor.\n";
//        }

        //if (governorValues.Contains(CHARACTER_VALUE.INFLUENCE)) {
        //    adjustment = -15;
        //    baseLoyalty += adjustment;
        //    this._loyaltySummary += adjustment.ToString() + "   values influence.\n";
        //}

        for (int i = 0; i < kingdom.relationships.Count; i++){
			if(kingdom.relationships.ElementAt(i).Value.isAtWar){
                adjustment = -10;
                baseLoyalty += adjustment;
                this._loyaltySummary += adjustment.ToString() + "   Kingdom is at war.\n";

//                int exhaustion = (int)(kingdom.relationshipsWithOtherKingdoms [i].kingdomWar.exhaustion / 10);
//				baseLoyalty -= exhaustion;
//                this._loyaltySummary += "-" + exhaustion.ToString() + "   War exhaustion.\n";
            }
		}
//		if(this.citizen.city.isRaided){
//            int adjustment = 10;
//            baseLoyalty -= adjustment;
//            this._loyaltySummary += "-" + adjustment.ToString() + "   Recent Raid.\n";
//        }
//		if(kingdom.hasConflicted){
//            int adjustment = 10;
//            baseLoyalty -= adjustment;
//            this._loyaltySummary += "-" + adjustment.ToString() + "   Recent Border Conflict.\n";
//        }
		if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility < 0){
            adjustment = -15;
            baseLoyalty += adjustment;
            this._loyaltySummary += adjustment.ToString() + "   King is husband of a relative and their compatibility is negative.\n";
        }

		this._loyalty = 0;
		this.AdjustLoyalty (baseLoyalty);
	}

	internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger) {
		GameDate dateToUse = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		dateToUse.AddMonths(3);
		ExpirableModifier expMod = new ExpirableModifier (gameEventTrigger, summary, dateToUse, modification);
		this._eventModifiers.Add(expMod);
        _eventLoyaltyModifier += modification;
        SchedulingManager.Instance.AddEntry (expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
		GovernorEvents ();
//        this._eventLoyaltyModifier += modification;
//        if(_eventLoyaltyModifier < 0) {
//            this._eventLoyaltySummary = "-" + _eventLoyaltyModifier.ToString() + "   Approval";
//        } else if (_eventLoyaltyModifier > 0) {
//            this._eventLoyaltySummary = "+" + _eventLoyaltyModifier.ToString() + "   Approval";
//        } else {
//            this._eventLoyaltySummary = _eventLoyaltyModifier.ToString() + "   Approval";
//        }
    }
//	private void UpdateEventModifiers(){
//		this._eventLoyaltyModifier = 0;
//		this._eventLoyaltySummary = string.Empty;
//		List<EVENT_TYPES> eventTypes = this._eventModifiers.Keys;
//		for (int i = 0; i < eventTypes.Count; i++) {
//			for (int j = 0; j < this._eventModifiers[eventTypes[i]].Count; j++) {
//				ExpirableModifier expMod = this._eventModifiers [eventTypes [i]] [j];
//				this._eventLoyaltyModifier += expMod.modifier;
//				if(expMod.modifier < 0){
//					this._eventLoyaltySummary += "-" + expMod.modifier.ToString() + " " + expMod.modifierReason;
//				}else{
//					this._eventLoyaltySummary += "+" + expMod.modifier.ToString() + " " + expMod.modifierReason;
//				}
//			}
//		}
//	}

	//private void CheckEventModifiers(){
	//	for (int i = 0; i < this._eventModifiers.Count; i++) {
	//		ExpirableModifier expMod = this._eventModifiers [i];
	//		if(expMod.dueDate.day == GameManager.Instance.days && expMod.dueDate.month == GameManager.Instance.month && expMod.dueDate.year == GameManager.Instance.year){
	//			RemoveEventModifierAt (i);
	//			i--;
	//		}

	//	}
	//}

	private void RemoveEventModifierAt(int index){
        ExpirableModifier modToRemove = this._eventModifiers[index];
        if (modToRemove.modifier < 0) {
            //if the modifier is negative, return the previously subtracted value
            _eventLoyaltyModifier += Mathf.Abs(modToRemove.modifier);
        } else {
            //if the modifier is positive, subtract the amount that was previously added
            _eventLoyaltyModifier -= modToRemove.modifier;
        }
        this._eventModifiers.RemoveAt (index);

	}
	private void RemoveEventModifier(ExpirableModifier expMod){
		if(!this.citizen.isDead){
			if (expMod.modifier < 0) {
				//if the modifier is negative, return the previously subtracted value
				_eventLoyaltyModifier += Mathf.Abs(expMod.modifier);
			} else {
				//if the modifier is positive, subtract the amount that was previously added
				_eventLoyaltyModifier -= expMod.modifier;
			}
			this._eventModifiers.Remove (expMod);		
		}
	}
	internal void SetLoyalty(int newLoyalty) {
        this._loyalty = newLoyalty;
    }

    internal void ResetEventModifiers() {
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;
    }

	private void GovernorEvents(){
		if(!this.isInitial){
			TriggerSecession ();
		}else{
			this.isInitial = false;
		}
	}
	private void TriggerSecession(){
		if(this.loyalty <= -50 && !this.citizen.city.kingdom.hasSecession){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 25){
				EventCreator.Instance.CreateSecessionEvent(this.citizen);
			}
		}
	}

	internal override void OnDeath (){
		base.OnDeath ();
		//PrestigeContribution (true);
		//StabilityContribution (true);
		//IntelligenceContribution (true);
//		Messenger.RemoveListener("OnDayEnd", CheckEventModifiers);
	}
}
