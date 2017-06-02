﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Raid : GameEvent {
	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;
	public City raidedCity;
	public List<Kingdom> otherKingdoms;
	public string pilfered;

	private bool hasBeenDiscovered;
	private bool hasDeflected;
	private bool hasDeath;
	private bool isSuccessful;
	private bool hasArrived;
	private Kingdom kingdomToBlame;

	internal Raider raider;
	public Raid(int startWeek, int startMonth, int startYear, Citizen startedBy, City raidedCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RAID;
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = startedBy.city.kingdom;
		this.targetKingdom = raidedCity.kingdom;
		this.raidedCity = raidedCity;
		this.hasBeenDiscovered = false;
		this.hasDeflected = false;
		this.hasDeath = false;
		this.isSuccessful = false;
		this.hasArrived = false;
		this.kingdomToBlame = null;

		this.otherKingdoms = GetOtherKingdoms ();
		

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "event_title");
		newLogTitle.AddToFillers (this.raidedCity, this.raidedCity.name);

		Log raidStartLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "start");
		raidStartLog.AddToFillers (this.startedByCity, this.startedByCity.name);
		raidStartLog.AddToFillers (this.raidedCity, this.raidedCity.name);

		DeflectBlame ();

		EventManager.Instance.AddEventToDictionary (this);
		this.EventIsCreated ();

	}
	internal void StartRaiding(){
		//Add logs: start_raiding

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}
	internal override void PerformAction(){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			ActualRaid ();
			DoneEvent ();
		}else{
			if(this.remainingDays < (this.durationInDays - 7)){
				if(!this.hasArrived){
					this.hasArrived = true;
					Arrival ();
				}
				RaidPartyDiscovery ();
				AccidentKilling ();
			}
		}
	}
	internal override void DeathByOtherReasons(){
		//Add logs: death_by_other

//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_other");
//		newLog.AddToFillers (this.startedBy, this.startedBy.name);
//		newLog.AddToFillers (null, this.startedBy.deathReasonText);
//
		this.DoneEvent ();
	}
	internal override void DeathByGeneral(General general){
		//Add logs: death_by_general

//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_general");
//		newLog.AddToFillers (general.citizen, general.citizen.name);

		this.startedBy.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent ();
	}
	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;

		if(this.hasBeenDiscovered){
			this._warTrigger = WAR_TRIGGER.DISCOVERED_RAID_NO_DEATH;
			if(this.hasDeath){
				this._warTrigger = WAR_TRIGGER.DISCOVERED_RAID_WITH_DEATH;
			}
			RelationshipKings relationship = this.GetRelationship ();
			if (relationship != null) {
				this.targetKingdom.king.WarTrigger (relationship, this, this.targetKingdom.kingdomTypeData);
			}
		}

		this.raider.DestroyGO ();
	}
	private List<Kingdom> GetOtherKingdoms(){
		if(this.raidedCity == null){
			return null;
		}
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.sourceKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.targetKingdom.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}

	//Raid party arrives at city
	private void Arrival(){
		if(this.raidedCity == null){
			return;
		}
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_arrival");
		newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
	}

	//Moment that raid party is going to steal from city
	private void ActualRaid(){
		if(this.raidedCity == null){
			return;
		}

		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 25){
			Steal ();
		} else {
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_fail");
		}
	}

	private void Steal(){
		this.isSuccessful = true;

		int stolenGold = (int)(this.raidedCity.goldCount * 0.20f);
		this.startedBy.city.goldCount += stolenGold;
		this.raidedCity.goldCount -= stolenGold;

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_success");
		newLog.AddToFillers (null, stolenGold.ToString());

		this.pilfered = string.Empty;
		this.pilfered += stolenGold.ToString() + " Gold";

	}

	//Whether or not the raid party can deflect the blame to another kingdom upon discovery
	private void DeflectBlame(){
		if (this.startedBy.hasTrait(TRAIT.SCHEMING)) {
			int deflectChance = UnityEngine.Random.Range (0, 100);
			if (deflectChance < 35) {
				Kingdom kingdomToBlame = GetRandomKingdomToBlame ();
				if (kingdomToBlame != null) {
					this.hasDeflected = true;
					this.kingdomToBlame = kingdomToBlame;
				}
			}
		}
	}

	//Accident killing of random citizen
	private void AccidentKilling(){
		if(this.raidedCity == null){
			return;
		}
		Citizen deadCitizen = null;
		bool isGovernor = false;
		bool isKing = false;
		int deathChance = UnityEngine.Random.Range (0, 100);
		if(deathChance < 2){
			List<Citizen> citizens = new List<Citizen> ();
			for(int i = 0; i < this.raidedCity.citizens.Count; i++){
				if(!this.raidedCity.citizens[i].isDead){
					citizens.Add (this.raidedCity.citizens [i]);
				}
			}
			if(citizens.Count > 0){
				deadCitizen = citizens [UnityEngine.Random.Range (0, citizens.Count)];
				isGovernor = deadCitizen.isGovernor;
				isKing = deadCitizen.isKing;
				this.hasDeath = true;
				deadCitizen.Death (DEATH_REASONS.INTERNATIONAL_WAR);
			}
		}
		if (deadCitizen != null) {
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_accident");
			newLog.AddToFillers (deadCitizen, deadCitizen.name);
			if(this.hasBeenDiscovered){
				int amountToAdjust = -15;
				if (isGovernor || isKing) {
					if (isGovernor) {
						amountToAdjust = -25;
					} else {
						amountToAdjust = -35;
					}
				}
				RelationshipKings relationship = this.GetRelationship ();
				if(relationship != null){
					relationship.AdjustLikeness(amountToAdjust, this);
				}

			}

		}

	}

	//Discovery of Raid Party which will cause relationship deterioration
	private void RaidPartyDiscovery(){
		if(this.raidedCity == null){
			return;
		}
		if(this.hasBeenDiscovered){
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 10){
			//DISCOVERY
			this.hasBeenDiscovered = true;
			if (this.hasDeflected) {
				if(this.kingdomToBlame == null || !this.kingdomToBlame.isAlive()){
					this.kingdomToBlame = GetRandomKingdomToBlame ();
					if(this.kingdomToBlame != null){
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_discovery_deflect");
						newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
						newLog.AddToFillers (this.kingdomToBlame, this.kingdomToBlame.name);
					}else{
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_discovery");
						newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
					}
				}
			}else {
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_discovery");
				newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
			}
			RelationshipKings relationship = this.GetRelationship ();
			if(relationship != null){
				relationship.AdjustLikeness(-10, this);
			}
		}
	}
	private Kingdom GetRandomKingdomToBlame(){
		if(this.otherKingdoms == null || this.otherKingdoms.Count <= 0){
			return null;
		}
		this.otherKingdoms.RemoveAll (x => !x.isAlive ());
		return this.otherKingdoms [UnityEngine.Random.Range (0, this.otherKingdoms.Count)];
	}
	private int GetRandomBasicResource(ref BASE_RESOURCE_TYPE resourceType){
		if(this.raidedCity.lumberCount > 0 && this.raidedCity.stoneCount > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.WOOD;
				return this.raidedCity.lumberCount;
			}else{
				resourceType = BASE_RESOURCE_TYPE.STONE;
				return this.raidedCity.stoneCount;
			}
		}else if(this.raidedCity.lumberCount <= 0 && this.raidedCity.stoneCount > 0){
			resourceType = BASE_RESOURCE_TYPE.STONE;
			return this.raidedCity.stoneCount;
		}else if(this.raidedCity.lumberCount > 0 && this.raidedCity.stoneCount <= 0){
			resourceType = BASE_RESOURCE_TYPE.WOOD;
			return this.raidedCity.lumberCount;
		}else{
			resourceType = BASE_RESOURCE_TYPE.NONE;
			return 0;
		}
	}

	private int GetRandomRareResource(ref BASE_RESOURCE_TYPE resourceType){
		if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount > 0){
			int chance = UnityEngine.Random.Range (0, 3);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
				return this.raidedCity.manaStoneCount;
			}else if(chance == 1){
				resourceType = BASE_RESOURCE_TYPE.MITHRIL;
				return this.raidedCity.mithrilCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.COBALT;
				return this.raidedCity.cobaltCount;
			}
		}else if(this.raidedCity.manaStoneCount <= 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MITHRIL;
				return this.raidedCity.mithrilCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.COBALT;
				return this.raidedCity.cobaltCount;
			}
		}else if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount <= 0 && this.raidedCity.cobaltCount > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
				return this.raidedCity.manaStoneCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.COBALT;
				return this.raidedCity.cobaltCount;
			}
		}else if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount <= 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
				return this.raidedCity.manaStoneCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.MITHRIL;
				return this.raidedCity.mithrilCount;
			}
		}else if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount <= 0 && this.raidedCity.cobaltCount <= 0){
			resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
			return this.raidedCity.manaStoneCount;
		}else if(this.raidedCity.manaStoneCount <= 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount <= 0){
			resourceType = BASE_RESOURCE_TYPE.MITHRIL;
			return this.raidedCity.mithrilCount;
		}else if(this.raidedCity.manaStoneCount <= 0 && this.raidedCity.mithrilCount <= 0 && this.raidedCity.cobaltCount > 0){
			resourceType = BASE_RESOURCE_TYPE.COBALT;
			return this.raidedCity.cobaltCount;
		}else{
			resourceType = BASE_RESOURCE_TYPE.NONE;
			return 0;
		}
	}

	private RelationshipKings GetRelationship(){
		RelationshipKings relationship = null;
		if(this.targetKingdom == null || !this.targetKingdom.isAlive()){
			return relationship;
		}
		relationship = this.targetKingdom.king.SearchRelationshipByID (this.sourceKingdom.king.id);
		if(this.hasDeflected){
			if(this.kingdomToBlame != null){
				if(this.kingdomToBlame.isAlive()){
					relationship = this.targetKingdom.king.SearchRelationshipByID (this.kingdomToBlame.king.id);
				}else{
					relationship = null;
				}
			}
		}
		return relationship;
	}

	internal override void CancelEvent (){

	}
}
