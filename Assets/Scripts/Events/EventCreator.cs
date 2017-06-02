﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventCreator: MonoBehaviour {
	public static EventCreator Instance;

	void Awake(){
		Instance = this;
	}
	internal Expansion CreateExpansionEvent(Kingdom kingdom){
		if(kingdom.capitalCity == null){
			return null;
		}
		HexTile hexTileToExpandTo = CityGenerator.Instance.GetNearestHabitableTile (kingdom.cities [0]);
		if(hexTileToExpandTo == null){
			return null;
		}
		Citizen expander = kingdom.capitalCity.CreateAgent (ROLE.EXPANDER, EVENT_TYPES.EXPANSION, hexTileToExpandTo, EventManager.Instance.eventDuration[EVENT_TYPES.EXPANSION]);
		if (expander != null) {
			Expansion expansion = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, expander, hexTileToExpandTo);
			expander.assignedRole.Initialize (expansion);
			return expansion;
		}
		return null;
	}

	internal Raid CreateRaidEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		City city = null;
		if(secondKingdom.cities.Count > 0){
			city = secondKingdom.cities [UnityEngine.Random.Range (0, secondKingdom.cities.Count)];
		}
		if(city == null){
			return null;
		}
		Citizen raider = firstKingdom.capitalCity.CreateAgent (ROLE.RAIDER, EVENT_TYPES.RAID, city.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.RAID]);
		if(raider != null){
			Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, city);
			raider.assignedRole.Initialize (raid);
			return raid;
		}
		return null;
	}
	internal BorderConflict CreateBorderConflictEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen startedBy = firstKingdom.king;
		BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, firstKingdom, secondKingdom);
		return borderConflict;
	}
	internal DiplomaticCrisis CreateDiplomaticCrisisEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen startedBy = secondKingdom.king;
		DiplomaticCrisis diplomaticCrisis = new DiplomaticCrisis(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, secondKingdom, firstKingdom);
		return diplomaticCrisis;
	}
	internal JoinWar CreateJoinWarEvent(Kingdom kingdom, Kingdom friend, InvasionPlan invasionPlan){
		Citizen envoy = kingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.JOIN_WAR_REQUEST, friend.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.JOIN_WAR_REQUEST]);
		Citizen citizenToPersuade = friend.king;
		if(envoy != null && citizenToPersuade != null){
			Envoy chosenEnvoy = (Envoy)envoy.assignedRole;
			JoinWar joinWar = new JoinWar (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, invasionPlan.startedBy, 
				citizenToPersuade, chosenEnvoy, invasionPlan.targetKingdom, invasionPlan);
			envoy.assignedRole.Initialize (joinWar);
			return joinWar;
		}
		return null;
	}
	internal StateVisit CreateStateVisitEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen visitor = secondKingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.STATE_VISIT, firstKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.STATE_VISIT]);
		if(visitor != null){
			Envoy chosenEnvoy = (Envoy)visitor.assignedRole;
			StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, secondKingdom, chosenEnvoy);
			visitor.assignedRole.Initialize (stateVisit);
			return stateVisit;
		}
		return null;
	}

    /*
     * Create a Trade Event
     * */
    internal Trade CreateTradeEvent(Kingdom sourceKingdom, Kingdom targetKingdom) {
        Citizen trader = sourceKingdom.capitalCity.CreateAgent(ROLE.TRADER, EVENT_TYPES.TRADE, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.TRADE]);
        if (trader != null) {
            Debug.Log("CREATED NEW TRADE EVENT: " + sourceKingdom.name + " towards " + targetKingdom.name);
            Trade tradeEvent = new Trade(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
                sourceKingdom.king, sourceKingdom, targetKingdom, trader);
            trader.assignedRole.Initialize(tradeEvent);
        }

        return null;
    }

	internal Sabotage CreateSabotageEvent(Kingdom sourceKingdom, Kingdom targetKingdom, GameEvent eventToSabotage, int remainingDays){
		Citizen envoy = sourceKingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.SABOTAGE, targetKingdom.capitalCity.hexTile, remainingDays);
		if(envoy != null){
			Envoy saboteur = (Envoy)envoy.assignedRole;
			Sabotage sabotage = new Sabotage(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				sourceKingdom.king, saboteur, eventToSabotage);
			envoy.assignedRole.Initialize (sabotage);
		}
		return null;
	}

	internal Assassination CreateAssassinationEvent(Kingdom sourceKingdom, Citizen targetCitizen, GameEvent gameEventTrigger, int remainingDays){
		HexTile targetLocation = targetCitizen.currentLocation;
		if(targetCitizen.assignedRole != null){
			if(targetCitizen.assignedRole.targetLocation != null){
				targetLocation = targetCitizen.assignedRole.targetLocation;
			}
		}
		Citizen spy = sourceKingdom.capitalCity.CreateAgent (ROLE.SPY, EVENT_TYPES.SABOTAGE, targetLocation, remainingDays);
		if(spy != null){
			Spy assassin = (Spy)spy.assignedRole;
			Assassination assassination = new Assassination(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				sourceKingdom.king, targetCitizen, assassin, gameEventTrigger);
			spy.assignedRole.Initialize (assassination);
		}
		return null;
	}
}
