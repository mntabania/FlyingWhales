﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarriageInvitation : GameEvent {

	public List<Citizen> elligibleCitizens;
	protected int goldForEvent;

	public MarriageInvitation(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.MARRIAGE_INVITATION;
		if (startedBy.gender == GENDER.MALE) {
			this.description = startedBy.name + " is looking for a suitable wife as the vessel of his heir";
		} else {
			this.description = startedBy.name + " is looking for a suitable husband.";
		}

		this.durationInWeeks = 8;
		this.remainingWeeks = this.durationInWeeks;
		this.goldForEvent = 0;
		this.GetGoldForEvent ();

		this.startedBy.city.hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;

			if (this.startedBy.isMarried && this.startedBy.spouse != null) {
				this.resolution = this.startedBy.name + " got married to " + this.startedBy.spouse.name + " in some other marriage invitation event.";
				this.DoneEvent();
				return;
			}
			this.elligibleCitizens = MarriageManager.Instance.GetElligibleCitizensForMarriage(this.startedBy);
		} else {
			if (this.startedBy.isMarried && this.startedBy.spouse != null) {
				this.resolution = this.startedBy.name + " got married to " + this.startedBy.spouse.name + " in some other marriage invitation event.";
				this.DoneEvent();
				return;
			}
			this.elligibleCitizens = MarriageManager.Instance.GetElligibleCitizensForMarriage(this.startedBy);
			//Choose bride
			if (this.elligibleCitizens.Count > 0) {
				Citizen chosenCitizen = this.elligibleCitizens[Random.Range(0, this.elligibleCitizens.Count)];
				City cityRecievingGold = null;
				if (chosenCitizen.city.id != this.startedBy.city.id) {
					//pay 500 gold to the chosen citizens city
					chosenCitizen.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, this.goldForEvent);
					cityRecievingGold = chosenCitizen.city;
				} else {
					//give back 500 gold to city
					this.startedBy.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, this.goldForEvent);
				}

				//move chosenCitizen to startedBy city if chosenCitizen is not king, else, move startedBy
				if (chosenCitizen.isKing) {
					this.MoveCitizenToCity (this.startedBy, chosenCitizen.city);
					for (int i = 0; i < this.startedBy.dependentChildren.Count; i++) {
						this.MoveCitizenToCity (this.startedBy.dependentChildren[i], chosenCitizen.city);
					}
				} else {
					this.MoveCitizenToCity (chosenCitizen, this.startedBy.city);
					for (int i = 0; i < chosenCitizen.dependentChildren.Count; i++) {
						this.MoveCitizenToCity (chosenCitizen.dependentChildren[i], this.startedBy.city);
					}
				}

				if (startedBy.father != null) {
					if (startedBy.father.isKing) {
						startedBy.city.kingdom.king.GetRelationshipWithCitizen(chosenCitizen.city.kingdom.king).AdjustLikeness(15);
					}
				}
				if (startedBy.mother != null) {
					if (startedBy.mother.isKing) {
						startedBy.city.kingdom.king.GetRelationshipWithCitizen(chosenCitizen.city.kingdom.king).AdjustLikeness(15);
					}
				}

				if (chosenCitizen.father != null) {
					if (chosenCitizen.father.isKing) {
						chosenCitizen.city.kingdom.king.GetRelationshipWithCitizen(startedBy.city.kingdom.king).AdjustLikeness(15);
					}
				}
				if (chosenCitizen.mother != null) {
					if (chosenCitizen.mother.isKing) {
						chosenCitizen.city.kingdom.king.GetRelationshipWithCitizen(startedBy.city.kingdom.king).AdjustLikeness(15);
					}
				}

				this.endWeek = GameManager.Instance.week;
				this.endMonth = GameManager.Instance.month;
				this.endYear = GameManager.Instance.year;
				if (startedBy.gender == GENDER.MALE) {
					this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endWeek.ToString () + ", " + this.endYear.ToString () + ". " + startedBy.name + " has selected " +
					chosenCitizen.name + " as his wife. ";
				} else {
					this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endWeek.ToString () + ", " + this.endYear.ToString () + ". " + startedBy.name + " has selected " +
						chosenCitizen.name + " as her husband. ";
				}
				if (cityRecievingGold != null) {
					this.resolution += cityRecievingGold.name + ", " +  chosenCitizen.name + "'s home city, recieved " + this.goldForEvent.ToString() + " Gold as compensation.";
				}

				MarriageManager.Instance.Marry(startedBy, chosenCitizen);
				this.DoneEvent();
				return;
			}

			this.endWeek = GameManager.Instance.week;
			this.endMonth = GameManager.Instance.month;
			this.endYear = GameManager.Instance.year;
			//return gold to city
			this.startedBy.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, 500);
			this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endWeek.ToString () + ", " + this.endYear.ToString () + ". " + startedBy.name + " was unable to marry.";
			this.DoneEvent();
			return;
		}
	}

	protected void GetGoldForEvent(){
		this.startedBy.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, -500);
		this.goldForEvent = 500;
	}
		
	protected void MoveCitizenToCity(Citizen citizenToMove, City targetCity){
		citizenToMove.city.RemoveCitizenFromCity(citizenToMove);
		targetCity.AddCitizenToCity(citizenToMove);
	}

	internal override void DoneEvent(){
		Debug.LogError (this.startedBy.name + "'s marriage invitation has ended. " + this.resolution);
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
}
