﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour {
	public static CombatManager Instance;

	void Awake(){
		Instance = this;
	}
	internal void CityBattle(City city, General generalAttacker){
		if(city == null || city.isDead){
			return;
		}
		General attackerGeneral = generalAttacker;
		General victoriousGeneral = null;
		General friendlyGeneral = null;
		List<General> attackers = city.incomingGenerals.Where (x => x.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && x.location == city.hexTile && x.assignedCampaign.targetCity.id == city.id && x.citizen.id != attackerGeneral.citizen.id).ToList();
		List<General> defenders = new List<General>();

		defenders.Clear ();
		defenders.AddRange (city.incomingGenerals.Where (x => x.assignedCampaign.campaignType == CAMPAIGN.DEFENSE && x.location == city.hexTile && x.citizen.city.governor.id != attackerGeneral.citizen.city.governor.id).ToList ());
		defenders.AddRange (city.incomingGenerals.Where (x => x.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && x.location == city.hexTile && x.assignedCampaign.rallyPoint == city.hexTile && x.citizen.city.governor.id != attackerGeneral.citizen.city.governor.id).ToList ());
		defenders.AddRange (city.GetAllGenerals (attackerGeneral));
		defenders = defenders.OrderByDescending (x => x.army.hp).ToList();
		if(defenders.Count > 0){
			List<General> newDefenders = new List<General> (defenders);
			GoToBattle (ref newDefenders, ref attackerGeneral);
			generalAttacker = attackerGeneral;
			defenders = newDefenders;
			if (defenders.Count <= 0 && generalAttacker.army.hp > 0) {
				//CITY DEFEATED AND CONQUERED --- IF ENEMY IS STILL ALIVE, AND YOU HAVE NO GENERALS LEFT
				victoriousGeneral = generalAttacker;
			}

		}
		if(defenders.Count <= 0){
			for (int i = 0; i < attackers.Count; i++) {
				if (victoriousGeneral == null) {
					victoriousGeneral = attackers [i];
				} else {
					friendlyGeneral = victoriousGeneral;
					if (friendlyGeneral.citizen.city.id != attackers [i].citizen.city.id) {
						if (attackers [i].assignedCampaign.warType == WAR_TYPE.INTERNATIONAL) {
							if (!Utilities.AreTwoGeneralsFriendly (attackers [i], friendlyGeneral)) {
								if (!Utilities.AreTwoGeneralsFriendly (friendlyGeneral, attackers [i])) {
									Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
									Battle (ref friendlyGeneral, ref attackerGeneral);
									attackers [i] = attackerGeneral;
									if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp <= 0) {
										victoriousGeneral = null;
									} else if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp > 0) {
										victoriousGeneral = friendlyGeneral;
									} else if (attackers [i].army.hp > 0 && friendlyGeneral.army.hp <= 0) {
										victoriousGeneral = attackers [i];
									}
									Debug.Log ("WINNER: " + victoriousGeneral.citizen.city.name);
								}
							}
						} else if (attackers [i].assignedCampaign.warType == WAR_TYPE.SUCCESSION) {
							if (attackers [i].assignedCampaign.leader.id != friendlyGeneral.assignedCampaign.leader.id) {
								if (!Utilities.AreTwoGeneralsFriendly (attackers [i], friendlyGeneral)) {
									if (!Utilities.AreTwoGeneralsFriendly (friendlyGeneral, attackers [i])) {
										Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
										Battle (ref friendlyGeneral, ref attackerGeneral);
										attackers [i] = attackerGeneral;
										if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp <= 0) {
											victoriousGeneral = null;
										} else if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp > 0) {
											victoriousGeneral = friendlyGeneral;
										} else if (attackers [i].army.hp > 0 && friendlyGeneral.army.hp <= 0) {
											victoriousGeneral = attackers [i];
										}
										Debug.Log ("WINNER: " + victoriousGeneral.citizen.city.name);
									}
								}
							}
						}
					}
				}
			}
			if(attackers.Count <= 0 || attackers == null){
				victoriousGeneral = generalAttacker;
			}
		}

		if(victoriousGeneral != null){
			Debug.Log (city.name + " IS DEFEATED BY " + victoriousGeneral.citizen.name + " of " + victoriousGeneral.citizen.city.name);
			if (victoriousGeneral.assignedCampaign.warType == WAR_TYPE.INTERNATIONAL) {
				for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
					KingdomManager.Instance.allKingdoms [i].king.campaignManager.intlWarCities.RemoveAll (x => x.city.id == city.id);
					KingdomManager.Instance.allKingdoms [i].king.campaignManager.defenseWarCities.RemoveAll (x => x.city.id == city.id);
				}
				EventManager.Instance.onRemoveSuccessionWarCity.Invoke (city);
			}

			for(int i = 0; i < city.incomingGenerals.Count; i++){
//				if (victoriousGeneral.assignedCampaign.warType == WAR_TYPE.INTERNATIONAL) {
//					if (city.incomingGenerals [i].citizen.id != victoriousGeneral.citizen.id) {
//						if (city.incomingGenerals [i].assignedCampaign.id != victoriousGeneral.assignedCampaign.id) {
//							Campaign campaign = city.incomingGenerals [i].assignedCampaign;
//							if (campaign != null) {
//								campaign.leader.campaignManager.CampaignDone (campaign);
//							}
//						}
//					}
//				} else 
				if (victoriousGeneral.assignedCampaign.warType == WAR_TYPE.SUCCESSION) {
					if (city.incomingGenerals [i].citizen.id != victoriousGeneral.citizen.id) {
						if (city.incomingGenerals [i].assignedCampaign.id == victoriousGeneral.assignedCampaign.id) {
							city.incomingGenerals [i].UnregisterThisGeneral ();
						}
					}
				}
			}
			victoriousGeneral.generalAvatar.gameObject.GetComponent<GeneralObject> ().Victory ();
		}
//		for(int i = 0; i < attackers.Count; i++){
//			General attackerGeneral = attackers [i];
//			defenders.Clear ();
//			defenders.AddRange (city.incomingGenerals.Where (x => x.assignedCampaign == CAMPAIGN.DEFENSE && x.location == city.hexTile && x.citizen.city.governor.id != attackerGeneral.citizen.city.governor.id).ToList ());
//			defenders.AddRange (city.incomingGenerals.Where (x => x.assignedCampaign == CAMPAIGN.OFFENSE && x.location == city.hexTile && x.rallyPoint == city.hexTile && x.citizen.city.governor.id != attackerGeneral.citizen.city.governor.id).ToList ());
//			defenders.AddRange (city.GetAllGenerals (attackerGeneral));
//			defenders = defenders.OrderByDescending (x => x.army.hp).ToList();
//			if(defenders.Count > 0){
//				List<General> newDefenders = new List<General> (defenders);
//				GoToBattle (ref newDefenders, ref attackerGeneral);
//				attackers [i] = attackerGeneral;
//				defenders = newDefenders;
//				if (defenders.Count <= 0 && attackers[i].army.hp > 0) {
//					//CITY DEFEATED AND CONQUERED --- IF ENEMY IS STILL ALIVE, AND YOU HAVE NO GENERALS LEFT
//					victoriousGeneral = attackers[i];
//					Debug.Log (city.name + " IS DEFEATED BY " + victoriousGeneral.citizen.city.name);
//				}
//			}else{
//				if(victoriousGeneral == null){
//					victoriousGeneral = attackers [i];
//				}else{
//					friendlyGeneral = victoriousGeneral;
//					if(friendlyGeneral.citizen.city.id != attackers[i].citizen.city.id){
//						if(attackers [i].assignedCampaign.warType == WAR_TYPE.INTERNATIONAL){
//							if (!Utilities.AreTwoGeneralsFriendly (attackers [i], friendlyGeneral)) {
//								if (!Utilities.AreTwoGeneralsFriendly (friendlyGeneral, attackers [i])) {
//									Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
//									Battle (ref friendlyGeneral, ref attackerGeneral);
//									attackers[i] = attackerGeneral;
//									if (attackers[i].army.hp <= 0 && friendlyGeneral.army.hp <= 0) {
//										victoriousGeneral = null;
//									} else if (attackers[i].army.hp <= 0 && friendlyGeneral.army.hp > 0) {
//										victoriousGeneral = friendlyGeneral;
//									} else if (attackers[i].army.hp > 0 && friendlyGeneral.army.hp <= 0) {
//										victoriousGeneral = attackers[i];
//									}
//									Debug.Log ("WINNER: " + victoriousGeneral.citizen.city.name);
//								}
//							}
//						}else if(attackers [i].assignedCampaign.warType == WAR_TYPE.SUCCESSION){
//							if(attackers [i].assignedCampaign.leader.id != friendlyGeneral.assignedCampaign.leader.id){
//								if(!Utilities.AreTwoGeneralsFriendly(attackers [i], friendlyGeneral)){
//									if(!Utilities.AreTwoGeneralsFriendly(friendlyGeneral, attackers [i])){
//										Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
//										Battle (ref friendlyGeneral, ref attackerGeneral);
//										attackers [i] = attackerGeneral;
//										if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp <= 0) {
//											victoriousGeneral = null;
//										} else if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp > 0) {
//											victoriousGeneral = friendlyGeneral;
//										} else if (attackers [i].army.hp > 0 && friendlyGeneral.army.hp <= 0) {
//											victoriousGeneral = attackers [i];
//										}
//										Debug.Log ("WINNER: " + victoriousGeneral.citizen.city.name);
//									}
//								}
//							}
//						}
//					}
//				}
//			}
//		}
//		if (victoriousGeneral != null) {
//			if (victoriousGeneral.assignedCampaign.warType == WAR_TYPE.INTERNATIONAL) {
//				for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
//					KingdomManager.Instance.allKingdoms [i].king.campaignManager.intlWarCities.RemoveAll (x => x.city.id == city.id);
//					KingdomManager.Instance.allKingdoms [i].king.campaignManager.defenseWarCities.RemoveAll (x => x.city.id == city.id);
//				}
//				EventManager.Instance.onRemoveSuccessionWarCity.Invoke (city);
//			}
//		}
//
//		city.incomingGenerals.RemoveAll(x => x.army.hp <= 0);
//		for(int i = 0; i < city.incomingGenerals.Count; i++){
//			if(victoriousGeneral != null){
//				if(victoriousGeneral.assignedCampaign.warType == WAR_TYPE.INTERNATIONAL){
//					if(city.incomingGenerals[i].assignedCampaign.warType == WAR_TYPE.INTERNATIONAL){
//						if (city.incomingGenerals[i].citizen.id != victoriousGeneral.citizen.id) {
//							if (city.incomingGenerals [i].assignedCampaign.id != victoriousGeneral.assignedCampaign.id) {
//								Campaign campaign = city.incomingGenerals [i].assignedCampaign.leader.campaignManager.SearchCampaignByID(city.incomingGenerals [i].assignedCampaign.id);
//								if (campaign != null) {
//									campaign.leader.campaignManager.CampaignDone (campaign);
//								}
//							}
//						}
//					}
//				}
//			}
//		}
//
//
//		if(victoriousGeneral != null){
//			Debug.Log (city.name + " IS DEFEATED BY " + victoriousGeneral.citizen.city.name);
////			if(((General)victoriousGeneral.assignedRole).warLeader.city.kingdom.id == city.kingdom.id){
////				//CIVIL WAR OR SUCCESSION WAR, SEARCH FOR TARGET
////			}
//			Campaign campaign = victoriousGeneral.warLeader.campaignManager.SearchCampaignByID (victoriousGeneral.campaignID);
//
//			if (campaign.warType == WAR_TYPE.INTERNATIONAL) {
//				if (campaign != null) {
//					campaign.leader.campaignManager.CampaignDone (campaign);
//				}
//
//
//				int countCitizens = city.citizens.Count;
//				for (int i = 0; i < countCitizens; i++) {
//					city.citizens [0].Death (DEATH_REASONS.INTERNATIONAL_WAR, false, null, true);
//				}
//				city.incomingGenerals.Clear ();
//				city.isDead = true;
//				ConquerCity (victoriousGeneral.citizen.city.kingdom, city);
//
//			} else if (campaign.warType == WAR_TYPE.SUCCESSION) {
//				victoriousGeneral.target = victoriousGeneral.warLeader.GetTargetSuccessionWar (campaign.targetCity);
//			}

//			for(int i = 0; i < city.incomingGenerals.Count; i++){
//				Campaign campaign = ((General)city.incomingGenerals[i].assignedRole).warLeader.campaignManager.SearchCampaignByID (((General)city.incomingGenerals[i].assignedRole).campaignID);
//				campaign.leader.campaignManager.CampaignDone (campaign);
//			}

//			victoriousGeneral.city.kingdomTile.kingdom.lord.RemoveMilitaryData (BATTLE_MOVE.ATTACK, this, null);
//			this.kingdomTile.kingdom.lord.RemoveMilitaryData (BATTLE_MOVE.DEFEND, null, victoriousGeneral);
//			victoriousGeneral.taskID = 0;
//			victoriousGeneral.task = GENERAL_TASK.NONE;
//			victoriousGeneral.targetCity = null;
//			victoriousGeneral.city.targetCity = null;
//			this.generals.Clear ();
//			this.isDead = true;
//			ConquerCity (victoriousGeneral.city.kingdomTile, this.visitingGenerals);
//		}
	}
	internal void GoToBattle(ref List<General> friendlyGenerals, ref General enemyGeneral){
		List<General> deadFriendlies = new List<General> ();

		General friendlyGeneral = null;

		for(int j = 0; j < friendlyGenerals.Count; j++){
			friendlyGeneral = friendlyGenerals [j];
			if(friendlyGeneral.army.hp > 0 && enemyGeneral.army.hp > 0){
				Battle (ref friendlyGeneral, ref enemyGeneral);
			}
			if (friendlyGeneral.army.hp <= 0) {
				deadFriendlies.Add (friendlyGeneral);
			}
			if(enemyGeneral.army.hp <= 0){
				break;
			}
		}

		for (int j = 0; j < deadFriendlies.Count; j++) {
			friendlyGenerals.Remove (deadFriendlies [j]);
		}
	}
//	internal void DeathByBattle(General general, City city){
//		city.incomingGenerals.Remove (general);
//		Campaign campaign = general.warLeader.campaignManager.SearchCampaignByID (general.campaignID);
//		if(campaign != null){
//			campaign.registeredGenerals.Remove (general);
//			if(campaign.registeredGenerals.Count <= 0){
//				campaign.leader.campaignManager.CampaignDone (campaign);
//			}
//		}
//	}
	internal void ConquerCity(Kingdom conqueror, City city){
		StartCoroutine(conqueror.ConquerCity(city));
	}
	internal void Battle(ref General general1, ref General general2, bool isMidway = false){
		Debug.Log ("BATTLE: (" + general1.citizen.city.name + ") " + general1.citizen.name + " and (" + general2.citizen.city.name + ") " + general2.citizen.name);
		Debug.Log ("enemy general army: " + general1.army.hp);
		Debug.Log ("friendly general army: " + general2.army.hp);

		float general1HPmultiplier = 1f;
		float general2HPmultiplier = 1f;

		if(!isMidway){
			if(general1.IsDefense(general2)){
				general1HPmultiplier = Utilities.defenseBuff;
			}

			if(general2.IsDefense(general1)){
				general2HPmultiplier = Utilities.defenseBuff;
			}
		}



		int general1TotalHP = (int)(general1.GetArmyHP() * general1HPmultiplier);
		int general2TotalHP = (int)(general2.GetArmyHP() * general2HPmultiplier);

		Debug.Log ("enemy total general army: " + general1TotalHP);
		Debug.Log ("friendly total general army: " + general2TotalHP);


		if(general1TotalHP > general2TotalHP){
			general1TotalHP -= general2TotalHP;
			general1.army.hp = general1TotalHP;
			general2.army.hp = 0;
//			general2.DeathArmy ();
		}else{
			general2TotalHP -= general1TotalHP;
			general2.army.hp = general2TotalHP;
			general1.army.hp = 0;
//			general1.DeathArmy ();
		}

		if(!isMidway){
			if(general1.army.hp <= 0){
				general1.DeathArmy ();
			}
			if(general2.army.hp <= 0){
				general2.DeathArmy ();
			}
		}

		RelationshipKingdom kingdomRelationshipToGeneral2 = general1.citizen.city.kingdom.GetRelationshipWithOtherKingdom(general2.citizen.city.kingdom);
		RelationshipKingdom kingdomRelationshipToGeneral1 = general2.citizen.city.kingdom.GetRelationshipWithOtherKingdom(general1.citizen.city.kingdom);


		if(general1.army.hp <= 0){
			//BATTLE LOST
			kingdomRelationshipToGeneral2.kingdomWar.battlesLost += 1;
			if(isMidway){
				kingdomRelationshipToGeneral2.AdjustExhaustion (10);
			}else{
				kingdomRelationshipToGeneral2.AdjustExhaustion (10);
			}


		}else{
			//BATTLE WON
			kingdomRelationshipToGeneral2.kingdomWar.battlesWon += 1;
			if(isMidway){
				kingdomRelationshipToGeneral2.AdjustExhaustion (-5);
			}else{
				kingdomRelationshipToGeneral2.AdjustExhaustion (-5);
			}
		}

		if(general2.army.hp <= 0){
			//BATTLE LOST
			kingdomRelationshipToGeneral1.kingdomWar.battlesLost += 1;
			if(isMidway){
				kingdomRelationshipToGeneral1.AdjustExhaustion (10);
			}else{
				kingdomRelationshipToGeneral1.AdjustExhaustion (10);
			}
		}else{
			//BATTLE WON
			kingdomRelationshipToGeneral1.kingdomWar.battlesWon += 1;
			if(isMidway){
				kingdomRelationshipToGeneral1.AdjustExhaustion (-5);
			}else{
				kingdomRelationshipToGeneral1.AdjustExhaustion (-10);
			}
		}

		int chanceToTriggerSendSpy = Random.Range (0, 100);
		if (chanceToTriggerSendSpy < 10) {
//		if (chanceToTriggerSendSpy < 100) {
			List<Kingdom> kingdom1Enemies = new List<Kingdom>();
			for (int j = 0; j < KingdomManager.Instance.allKingdoms.Count; j++) {
				Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[j];
				if (currentKingdom.id != general1.citizen.city.kingdom.id && currentKingdom.id != general2.citizen.city.kingdom.id) {
					if (currentKingdom.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.ENEMY ||
						currentKingdom.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						kingdom1Enemies.Add(currentKingdom);
					}
				}
			}

			List<Kingdom> kingdom2Enemies = new List<Kingdom>();
			for (int j = 0; j < KingdomManager.Instance.allKingdoms.Count; j++) {
				Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[j];
				if (currentKingdom.id != general1.citizen.city.kingdom.id && currentKingdom.id != general2.citizen.city.kingdom.id) {
					if (currentKingdom.king.GetRelationshipWithCitizen (general2.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.ENEMY ||
						currentKingdom.king.GetRelationshipWithCitizen (general2.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						kingdom2Enemies.Add(currentKingdom);
					}
				}
			}

			List<Kingdom> possibleKingdomsToSendSpy = kingdom1Enemies.Except (kingdom2Enemies).Union(kingdom2Enemies.Except (kingdom1Enemies)).ToList();

			for (int i = 0; i < possibleKingdomsToSendSpy.Count; i++) {
				int chance = Random.Range(0, 100);
				Kingdom possibleKingdomToTrigger = possibleKingdomsToSendSpy[i];
				List<Citizen> spies = possibleKingdomToTrigger.GetAllCitizensOfType (ROLE.SPY).Where(x => !((Spy)x.assignedRole).inAction).ToList();
				if (spies.Count > 0 && chance < 5) {
//				if (spies.Count > 0 && chance < 100) {
					Debug.Log ("Send spy to decrease tension!");
					//Send spy to kingdom that is not enemy
					if (possibleKingdomToTrigger.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.ENEMY ||
						possibleKingdomToTrigger.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						((Spy)spies [0].assignedRole).StartDecreaseWarExhaustionTask (kingdomRelationshipToGeneral1);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent a spy(" + spies [0].name + ") to " + general2.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));
						
						Debug.Log(possibleKingdomToTrigger.name + " sent a spy(" + spies[0].name + ") to " + general2.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name);
					} else {
						((Spy)spies [0].assignedRole).StartDecreaseWarExhaustionTask (kingdomRelationshipToGeneral2);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent a spy(" + spies [0].name + ") to " + general1.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

						Debug.Log(possibleKingdomToTrigger.name + " sent a spy(" + spies[0].name + ") to " + general1.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name);
					}
				}
			}
		}

		int chanceToTriggerSendEnvoy = Random.Range (0, 100);
		if (chanceToTriggerSendEnvoy < 10) {
//		if (chanceToTriggerSendEnvoy < 100) {
			List<Kingdom> kingdom1Friends = new List<Kingdom>();
			for (int j = 0; j < KingdomManager.Instance.allKingdoms.Count; j++) {
				Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[j];
				if (currentKingdom.id != general1.citizen.city.kingdom.id && currentKingdom.id != general2.citizen.city.kingdom.id) {
					if (currentKingdom.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.FRIEND ||
						currentKingdom.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.ALLY) {
						kingdom1Friends.Add(currentKingdom);
					}
				}
			}

			List<Kingdom> kingdom2Friends = new List<Kingdom>();
			for (int j = 0; j < KingdomManager.Instance.allKingdoms.Count; j++) {
				Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[j];
				if (currentKingdom.id != general1.citizen.city.kingdom.id && currentKingdom.id != general2.citizen.city.kingdom.id) {
					if (currentKingdom.king.GetRelationshipWithCitizen (general2.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.FRIEND ||
						currentKingdom.king.GetRelationshipWithCitizen (general2.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.ALLY) {
						kingdom2Friends.Add(currentKingdom);
					}
				}
			}

			List<Kingdom> commonFriends = kingdom1Friends.Intersect(kingdom2Friends).ToList();
			for (int i = 0; i < commonFriends.Count; i++) {
				int chance = Random.Range(0, 100);
				Kingdom possibleKingdomToTrigger = commonFriends[i];
				List<Citizen> envoys = possibleKingdomToTrigger.GetAllCitizensOfType (ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
				if (envoys.Count > 0 && chance < 5) {
//				if (envoys.Count > 0 && chance < 100) {
					if (Random.Range (0, 2) == 0) {
						((Envoy)envoys [0].assignedRole).StartIncreaseWarExhaustionTask (kingdomRelationshipToGeneral1);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent an envoy(" + envoys [0].name + ") to " + general2.citizen.city.kingdom.name + " to increase exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

						Debug.Log(possibleKingdomToTrigger.name + " sent an envoy(" + envoys[0].name + ") to " + general2.citizen.city.kingdom.name + " to increase exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name);
					} else {
						((Envoy)envoys [0].assignedRole).StartIncreaseWarExhaustionTask (kingdomRelationshipToGeneral2);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent an envoy(" + envoys [0].name + ") to " + general1.citizen.city.kingdom.name + " to increase exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

						Debug.Log(possibleKingdomToTrigger.name + " sent a envoy(" + envoys[0].name + ") to " + general1.citizen.city.kingdom.name + " to increase exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name);
					}
				}
			}

		}

		Debug.Log ("RESULTS: " + general1.citizen.name + " army hp left: " + general1.army.hp + "\n" + general2.citizen.name + " army hp left: " + general2.army.hp);
	}

	internal void BattleMidway(ref General general1, ref General general2){
		//MID WAY BATTLE IF supported is not the same
		Debug.Log("BATTLE MIDWAY!");

		General firstGeneral = general1;
		General secondGeneral = general2;

		Battle(ref firstGeneral, ref secondGeneral, true);

		general1 = firstGeneral;
		general2 = secondGeneral;

//		if(general1.army.hp <= 0){
//			if (general1.generalAvatar != null) {
//				GameObject.Destroy (general1.generalAvatar);
//				general1.generalAvatar = null;
//			}
//			general1.citizen.Death(DEATH_REASONS.BATTLE);
//		}
//
//		if(general2.army.hp <= 0){
//			if (general2.generalAvatar != null) {
//				GameObject.Destroy (general2.generalAvatar);
//				general2.generalAvatar = null;
//			}
//			general2.citizen.Death(DEATH_REASONS.BATTLE);
//		}
	}
}
