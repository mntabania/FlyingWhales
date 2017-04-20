﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomManager : MonoBehaviour {

	public static KingdomManager Instance = null;

	public List<Kingdom> allKingdoms;

	void Awake(){
		Instance = this;
	}

	public void GenerateInitialKingdoms(List<HexTile> elligibleTiles){
		List<HexTile> habitableTiles = new List<HexTile> (elligibleTiles);

		Debug.Log ("Generate Initial Kingdoms");
		//Get Starting City For Humans
		List<HexTile> cityForHumans1 = new List<HexTile>();
		List<HexTile> cityForHumans2 = new List<HexTile>();
		List<HexTile> cityForHumans3 = new List<HexTile>();
		List<HexTile> cityForHumans4 = new List<HexTile>();

		List<HexTile> elligibleTilesForHumans = new List<HexTile>();
		for (int i = 0; i < habitableTiles.Count; i++) {
			
			List<HexTile> neighbours = habitableTiles[i].AllNeighbours.ToList();
			List<HexTile> tilesContainingBaseResource = new List<HexTile>();
			for (int j = 0; j < neighbours.Count; j++) {
				if (neighbours[j].specialResource == RESOURCE.NONE) {
					if (Utilities.GetBaseResourceType (neighbours[j].defaultResource) == BASE_RESOURCE_TYPE.STONE) {
						tilesContainingBaseResource.Add(neighbours[j]);
					}
				} else {
					if (Utilities.GetBaseResourceType (neighbours[j].specialResource) == BASE_RESOURCE_TYPE.STONE) {
						tilesContainingBaseResource.Add(neighbours[j]);
					}
				}
			}
			if (tilesContainingBaseResource.Count > 0) {
				elligibleTilesForHumans.Add(habitableTiles[i]);
			}
		}

		if (elligibleTilesForHumans.Count > 2) {
			cityForHumans1.Add (elligibleTilesForHumans [0]);
			cityForHumans1.Add (elligibleTilesForHumans [1]);
			GenerateNewKingdom (RACE.HUMANS, cityForHumans1, true);
		}

		if (elligibleTilesForHumans.Count > 4) {
			cityForHumans2.Add (elligibleTilesForHumans[2]);
			cityForHumans2.Add (elligibleTilesForHumans[3]);
			GenerateNewKingdom (RACE.HUMANS, cityForHumans2, true);
		}

		if (elligibleTilesForHumans.Count > 6) {
			cityForHumans3.Add (elligibleTilesForHumans [4]);
			cityForHumans3.Add (elligibleTilesForHumans [5]);
			GenerateNewKingdom (RACE.HUMANS, cityForHumans3, true);
		}

//		if (elligibleTilesForHumans.Count > 7) {
//			cityForHumans4.Add (elligibleTilesForHumans [6]);
//			GenerateNewKingdom (RACE.HUMANS, cityForHumans4, true);
//		}

		for (int i = 0; i < elligibleTilesForHumans.Count; i++) {
			habitableTiles.Remove (elligibleTilesForHumans[i]);
		}

//		//Get Statrting City For Elves
//		List<HexTile> cityForElves = new List<HexTile>();
//		List<HexTile> elligibleTilesForElves = new List<HexTile>();
//		for (int i = 0; i < habitableTiles.Count; i++) {
//
//			List<HexTile> neighbours = habitableTiles[i].AllNeighbours.ToList();
//			List<HexTile> tilesContainingBaseResource = new List<HexTile>();
//			for (int j = 0; j < neighbours.Count; j++) {
//				if (neighbours[j].specialResource == RESOURCE.NONE) {
//					if (Utilities.GetBaseResourceType (neighbours[j].defaultResource) == BASE_RESOURCE_TYPE.WOOD) {
//						tilesContainingBaseResource.Add(neighbours[j]);
//					}
//				} else {
//					if (Utilities.GetBaseResourceType (neighbours[j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
//						tilesContainingBaseResource.Add(neighbours[j]);
//					}
//				}
//			}
//
//			if (tilesContainingBaseResource.Count > 0) {
//				elligibleTilesForElves.Add(habitableTiles[i]);
//			}
//		}
//		cityForElves.Add (elligibleTilesForElves [Random.Range (0, elligibleTilesForElves.Count)]);
//		GenerateNewKingdom (RACE.ELVES, cityForElves, true);
		CreateInitialRelationshipKings ();
	}

	internal void CreateInitialRelationshipKings(){
		for(int i = 0; i < this.allKingdoms.Count; i++){
			this.allKingdoms [i].king.CreateInitialRelationshipsToKings ();
		}
	}
	public void GenerateNewKingdom(RACE race, List<HexTile> cities, bool isForInitial = false){
		Kingdom newKingdom = new Kingdom (race, cities);
		allKingdoms.Add(newKingdom);
		EventManager.Instance.onCreateNewKingdomEvent.Invoke(newKingdom);
		if (isForInitial) {
			for (int i = 0; i < cities.Count; i++) {
				if (i == 0) {
					cities [i].city.CreateInitialFamilies();
				} else {
					cities [i].city.CreateInitialFamilies(false);
				}
			}
		}

		this.UpdateKingdomAdjacency();
	}

	public void DeclareWarBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		kingdom1.GetRelationshipWithOtherKingdom(kingdom2).isAtWar = true;
		kingdom2.GetRelationshipWithOtherKingdom(kingdom1).isAtWar = true;

		kingdom1.AddInternationalWar(kingdom2);
		kingdom2.AddInternationalWar(kingdom1);

		kingdom1.king.history.Add(new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, kingdom1.king.name + " of " + kingdom1.name + " declares war against " + kingdom2.name + ".", HISTORY_IDENTIFIER.NONE));
		kingdom2.king.history.Add(new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, kingdom2.king.name + " of " + kingdom2.name + " declares war against " + kingdom1.name + ".", HISTORY_IDENTIFIER.NONE));

	}

	public void DeclarePeaceBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		kingdom1.GetRelationshipWithOtherKingdom(kingdom2).isAtWar = false;
		kingdom2.GetRelationshipWithOtherKingdom(kingdom1).isAtWar = false;

		kingdom1.RemoveInternationalWar(kingdom2);
		kingdom2.RemoveInternationalWar(kingdom1);
	}

	public void AddRelationshipToOtherKings(Citizen newKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != newKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.Add (new RelationshipKings(this.allKingdoms[i].king, newKing, 0));
			}
		}
	}
	public void RemoveRelationshipToOtherKings(Citizen oldKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != oldKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.RemoveAll (x => x.king.id == oldKing.id);
			}
		}
	}

	public void MakeKingdomDead(Kingdom kingdomToDie){
		this.allKingdoms.Remove(kingdomToDie);
		RemoveRelationshipToOtherKingdoms (kingdomToDie);
	}

	public void RemoveRelationshipToOtherKingdoms(Kingdom kingdomToRemove){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			for (int j = 0; j < this.allKingdoms[i].relationshipsWithOtherKingdoms.Count; j++) {
				if (this.allKingdoms[i].relationshipsWithOtherKingdoms[j].objectInRelationship.id == kingdomToRemove.id) {
					this.allKingdoms[i].relationshipsWithOtherKingdoms.RemoveAt(j);
					break;
				}
			}
		}
	}

	public void UpdateKingdomAdjacency(){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			Kingdom currentKingdom = this.allKingdoms[i];
			currentKingdom.ResetAdjacencyWithOtherKingdoms();
			for (int j = 0; j < currentKingdom.cities.Count; j++) {
				City currentCity = currentKingdom.cities[j];
				for (int k = 0; k < currentCity.hexTile.connectedTiles.Count; k++) {
					HexTile currentConnectedTile = currentCity.hexTile.connectedTiles[k];
					if (currentConnectedTile.isOccupied && currentConnectedTile.city != null) {
						if (currentConnectedTile.city.kingdom.id != currentKingdom.id) {
							currentKingdom.GetRelationshipWithOtherKingdom(currentConnectedTile.city.kingdom).isAdjacent = true;
							currentConnectedTile.city.kingdom.GetRelationshipWithOtherKingdom(currentKingdom).isAdjacent = true;
						}
					}
				}
			}
		}
	}
}
