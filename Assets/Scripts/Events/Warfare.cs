﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Warfare {
	private int _id;
	private List<Kingdom> _sideA;
	private List<Kingdom> _sideB;
	private List<Battle> _battles;
	private List<Log> _logs;

	#region getters/setters
	public int id{
		get { return this._id; }
	}
	#endregion
	public Warfare(Kingdom firstKingdom, Kingdom secondKingdom){
		SetID();
		this._sideA = new List<Kingdom>();
		this._sideB = new List<Kingdom>();
		this._battles = new List<Battle>();
		JoinWar(WAR_SIDE.A, firstKingdom, false);
		JoinWar(WAR_SIDE.B, secondKingdom, false);
		CreateNewBattle (firstKingdom, true);
	}
	private void SetID(){
		this._id = Utilities.lastWarfareID + 1;
		Utilities.lastWarfareID = this._id;
	}

	internal void JoinWar(WAR_SIDE side, Kingdom kingdom, bool isCreateBattle = true){
		if(side == WAR_SIDE.A){
			this._sideA.Add(kingdom);
		}else if(side == WAR_SIDE.B){
			this._sideB.Add(kingdom);
		}
		kingdom.SetWarfareInfo(new WarfareInfo(side, this));
		if(isCreateBattle){
			CreateNewBattle (kingdom, true);
		}
	}
	internal void UnjoinWar(WAR_SIDE side, Kingdom kingdom){
		if(side == WAR_SIDE.A){
			this._sideA.Remove(kingdom);
		}else if(side == WAR_SIDE.B){
			this._sideB.Remove(kingdom);
		}
		kingdom.SetWarfareInfoToDefault();
	}
	internal void BattleEnds(City winnerCity, City loserCity, Battle battle){
		//Conquer City if not null, if null means both dead
		RemoveBattle (battle);
		if(winnerCity != null && loserCity != null){
			Log newLog = CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "invade");
			newLog.AddToFillers (winnerCity.kingdom, winnerCity.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (loserCity, loserCity.name, LOG_IDENTIFIER.CITY_2);
			ShowUINotificaiton (newLog);

			winnerCity.kingdom.ConquerCity(loserCity);
			CreateNewBattle (winnerCity.kingdom);
		}
	}
	internal void CreateNewBattle(Kingdom kingdom, bool isFirst = false){
		if(isFirst){
			City friendlyCity = null;
			City enemyCity = GetEnemyCity (kingdom);
			if(enemyCity != null){
				for (int i = 0; i < enemyCity.region.adjacentRegions.Count; i++) {
					City city = enemyCity.region.adjacentRegions [i].occupant;
					if(city != null && city.kingdom.id == kingdom.id){
						friendlyCity = city;
						break;
					}
				}
			}
			if(friendlyCity != null){
				Battle newBattle = new Battle (this, friendlyCity, enemyCity);
				AddBattle (newBattle);
			}
		}else{
			City friendlyCity = null;
			City enemyCity = null;
			List<City> nonRebellingCities = kingdom.nonRebellingCities;
			if(nonRebellingCities.Count > 0){
				friendlyCity = nonRebellingCities[nonRebellingCities.Count - 1];
			}
			if(friendlyCity != null){
				enemyCity = GetEnemyCity (friendlyCity);
			}
			if(enemyCity != null){
				
			}else{
				enemyCity = GetEnemyCity (kingdom);
				if (enemyCity != null) {
					Battle newBattle = new Battle (this, friendlyCity, enemyCity);
					AddBattle (newBattle);
				}
			}
		}
	}

	private City GetEnemyCity(City sourceCity){
		List<City> enemyCities = new List<City> ();
		for (int j = 0; j < sourceCity.region.adjacentRegions.Count; j++) {
			City adjacentCity = sourceCity.region.adjacentRegions [j].occupant;
			if(adjacentCity != null){
				if(adjacentCity.kingdom.warfareInfo.side != WAR_SIDE.NONE && adjacentCity.kingdom.warfareInfo.warfare != null){
					if(adjacentCity.kingdom.warfareInfo.side != sourceCity.kingdom.warfareInfo.side && adjacentCity.kingdom.warfareInfo.warfare.id == sourceCity.kingdom.warfareInfo.warfare.id){
						if(!enemyCities.Contains(adjacentCity)){
							enemyCities.Add (adjacentCity);
						}
					}
				}
			}
		}
		int lowestDef = enemyCities.Min (x => x.defense);
		for (int i = 0; i < enemyCities.Count; i++) {
			if(enemyCities[i].defense == lowestDef){
				return enemyCities [i];
			}
		}
		return null;
	}
	private City GetEnemyCity(Kingdom sourceKingdom){
		List<City> enemyCities = new List<City> ();
		for (int i = 0; i < sourceKingdom.cities.Count; i++) {
			if(!sourceKingdom.cities[i].isUnderAttack){
				for (int j = 0; j < sourceKingdom.cities[i].region.adjacentRegions.Count; j++) {
					City adjacentCity = sourceKingdom.cities [i].region.adjacentRegions [j].occupant;
					if(adjacentCity != null){
						if(adjacentCity.kingdom.warfareInfo.side != WAR_SIDE.NONE && adjacentCity.kingdom.warfareInfo.warfare != null){
							if(adjacentCity.kingdom.warfareInfo.side != sourceKingdom.warfareInfo.side && adjacentCity.kingdom.warfareInfo.warfare.id == sourceKingdom.warfareInfo.warfare.id){
								if(!enemyCities.Contains(adjacentCity)){
									enemyCities.Add (adjacentCity);
								}
							}
						}
					}
				}
			}
		}
		int lowestDef = enemyCities.Min (x => x.defense);
		for (int i = 0; i < enemyCities.Count; i++) {
			if(enemyCities[i].defense == lowestDef){
				return enemyCities [i];
			}
		}
		return null;
	}

	private void AddBattle(Battle battle){
		this._battles.Add (battle);
	}
	private void RemoveBattle(Battle battle){
		this._battles.Remove (battle);
	}

	internal Log CreateNewLogForEvent(int month, int day, int year, string category, string file, string key){
		Log newLog = new Log (month, day, year, category, file, key);
		this._logs.Add (newLog);
		return newLog;
	}
	internal void ShowUINotificaiton(Log log){
		UIManager.Instance.ShowNotification(log);
	}
}
