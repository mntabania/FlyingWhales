﻿using UnityEngine;
using System.Collections;

public class Riot : GameEvent {

	public Rebel rebel;
	internal Kingdom sourceKingdom;
	public Riot(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RIOT;
		//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = startedBy.city.kingdom;
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

		//		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
		//		newLogTitle.AddToFillers (visitor.citizen, visitor.citizen.name);
		//		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		//
		//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
		//		newLog.AddToFillers (visitor.citizen, visitor.citizen.name);
		//		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		//		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);

		//		EventManager.Instance.AddEventToDictionary (this);
		//		this.EventIsCreated ();

	}

	#region Overrides
	internal override void PerformAction (){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			DoneEvent ();
		}else{
			if(!this.sourceKingdom.isAlive()){
				CancelEvent ();
				return;
			}
			AttemptToDestroyStructure ();
		}
	}
//	internal override void DoneCitizenAction (Citizen citizen){
//		base.DoneCitizenAction(citizen);
//		if(this.saboteur != null){
//			if(citizen.id == this.saboteur.citizen.id){
//				AttemptToSabotage();
//			}
//		}
//	}
//	internal override void DeathByOtherReasons(){
//		this.DoneEvent();
//	}
//	internal override void DeathByGeneral(General general){
//		this.saboteur.citizen.Death (DEATH_REASONS.BATTLE);
//		this.DoneEvent();
//	}
	internal override void DoneEvent(){
		base.DoneEvent();
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
	private void AttemptToDestroyStructure(){
		int chance = UnityEngine.Random.Range(0, 100);
		if(chance < 3){
			City chosenCity = this.sourceKingdom.cities[UnityEngine.Random.Range(0, this.sourceKingdom.cities.Count)];
			DestroyStructure (chosenCity);
		}
	}

	private void DestroyStructure(City city){
		//Destroy a structure
	}
}
