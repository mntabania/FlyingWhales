using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class TileFeatureGeneration : MapGenerationComponent {

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Tile Features...");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		GenerateFeaturesForAllTiles(data);
		stopwatch.Stop();
		AddLog($"GenerateFeaturesForAllTiles took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
		
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			DetermineSettlementsForTutorial();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			DetermineSettlementsForOona(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			DetermineSettlementsForPangatLoo(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			DetermineSettlementsForZenko(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			DetermineSettlementsForAffatt(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			DetermineSettlementsForIcalawa(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
			DetermineSettlementsForAneem(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
			DetermineSettlementsForPitto(data);
		} else {
			stopwatch.Reset();
			stopwatch.Start();
			yield return MapGenerator.Instance.StartCoroutine(DetermineVillageSpots(data));
			stopwatch.Stop();
			AddLog($"DetermineVillageSpots took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
			
			stopwatch.Reset();
			stopwatch.Start();
			succeess = TryAssignSettlementTiles(data);
			stopwatch.Stop();
			AddLog($"TryAssignSettlementTiles took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
			
			// AdditionalResourceCreation();
		}
	}
	private void GenerateFeaturesForAllTiles(MapGenerationData data) {
		// Stopwatch stopwatch = new Stopwatch();
		// stopwatch.Start();
		// List<Area> flatTilesWithNoFeatures = RuinarchListPool<Area>.Claim();
		// int batchCount = 0;
		// for (int x = 0; x < GridMap.Instance.width; x++) {
		// 	for (int y = 0; y < GridMap.Instance.height; y++) {
		// 		Area tile = GridMap.Instance.map[x, y];
		// 		if (tile.elevationType == ELEVATION.PLAIN) {
		// 			if (GameUtilities.RollChance(30)) {
		// 				tile.featureComponent.AddFeature(AreaFeatureDB.Wood_Source_Feature, tile);	
		// 			} else if (tile.featureComponent.features.Count == 0) {
		// 				flatTilesWithNoFeatures.Add(tile);	
		// 			}
		// 		} else if (tile.elevationType == ELEVATION.MOUNTAIN) {
		// 			tile.featureComponent.AddFeature(AreaFeatureDB.Metal_Source_Feature, tile);	
		// 		}
		// 		batchCount++;
		// 		if (batchCount >= MapGenerationData.WorldMapFeatureGenerationBatches) {
		// 			batchCount = 0;
		// 			yield return null;
		// 		}
		// 	}	
		// }
		// stopwatch.Stop();
		// AddLog($"Determine flatTilesWithNoFeatures took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");

		// int stoneSourceCount = Random.Range(1, 4);
		// int fertileCount = Random.Range(1, 4);
		// int gameCount = Random.Range(1, 4);
		// int poisonVentsCount = Random.Range(0, 5);
		// int vaporVentsCount = Random.Range(0, 5);

		// //stone source
		// for (int i = 0; i < stoneSourceCount; i++) {
		// 	if (flatTilesWithNoFeatures.Count <= 0) { break; }
		// 	Area tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
		// 	tile.featureComponent.AddFeature(AreaFeatureDB.Stone_Source_Feature, tile);
		// 	flatTilesWithNoFeatures.Remove(tile);
		// }		
		//
		// yield return null;
		//
		// //fertile
		// for (int i = 0; i < fertileCount; i++) {
		// 	if (flatTilesWithNoFeatures.Count <= 0) { break; }
		// 	Area tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
		// 	tile.featureComponent.AddFeature(AreaFeatureDB.Fertile_Feature, tile);
		// 	flatTilesWithNoFeatures.Remove(tile);
		// }
		//
		// yield return null;

		// List<Area> ventChoices = RuinarchListPool<Area>.Claim();
		// ventChoices.AddRange(GridMap.Instance.allAreas);
		// //poison vents
		// for (int i = 0; i < poisonVentsCount; i++) {
		// 	if (ventChoices.Count == 0) { break; }
		// 	Area tile = CollectionUtilities.GetRandomElement(ventChoices);
		// 	tile.featureComponent.AddFeature(AreaFeatureDB.Poison_Vents, tile);
		// 	ventChoices.Remove(tile);
		// }
		// //vapor vents
		// for (int i = 0; i < vaporVentsCount; i++) {
		// 	if (ventChoices.Count == 0) { break; }
		// 	Area tile = CollectionUtilities.GetRandomElement(ventChoices);
		// 	tile.featureComponent.AddFeature(AreaFeatureDB.Vapor_Vents, tile);
		// 	ventChoices.Remove(tile);
		// }

		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			//pigs
			Area pigTile = GridMap.Instance.map[2, 4];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);

			//sheep
			Area sheepTile = GridMap.Instance.map[4, 3];
			GameFeature sheepGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			sheepGameFeature.SetSpawnType(SUMMON_TYPE.Sheep);
			sheepTile.featureComponent.AddFeature(sheepGameFeature, sheepTile);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			//Add 2 pigs 2 tiles away from village 
			Area pigTile = GridMap.Instance.map[5, 5];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
			//Add 2 pigs 2 tiles away from village 
			Area pigTile = GridMap.Instance.map[5, 5];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Chicken);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
		}
	}
	private IEnumerator DetermineVillageSpots(MapGenerationData p_data) {
		List<Area> villageSpotChoices = RuinarchListPool<Area>.Claim();
		villageSpotChoices.AddRange(GridMap.Instance.allAreas);
		List<Area> unreservedAreas = RuinarchListPool<Area>.Claim();
		unreservedAreas.AddRange(GridMap.Instance.allAreas);
		
		//Keep track of already reserved areas, so that they cannot be reserved by other village spots
		List<Area> alreadyReservedAreas = RuinarchListPool<Area>.Claim(); 

		int villageSpotsToCreate = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxVillagesForMapSize();
		while (p_data.villageSpots.Count < villageSpotsToCreate) {
			if (villageSpotChoices.Count == 0) { break; }
			Area mainSpotCandidate = CollectionUtilities.GetRandomElement(villageSpotChoices);
			if (IsAreaValidVillageSpotCandidate(mainSpotCandidate, p_data)) {
				villageSpotChoices.Remove(mainSpotCandidate);
				//must be connected to at least 6 unreserved contiguous flat external tiles
				List<Area> connectedPlainAreas = RuinarchListPool<Area>.Claim();
				connectedPlainAreas.Add(mainSpotCandidate);

				List<Area> areasToCheck = RuinarchListPool<Area>.Claim();
				List<Area> checkedAreas = RuinarchListPool<Area>.Claim();
				List<Area> waterAreas = RuinarchListPool<Area>.Claim();
				List<Area> caveAreas = RuinarchListPool<Area>.Claim();
				areasToCheck.AddRange(mainSpotCandidate.neighbourComponent.cardinalNeighbours);
				checkedAreas.Add(mainSpotCandidate);

				int lumberyardSpots = 0;
				int miningSpots = 0;
				
				while (areasToCheck.Count > 0) {
					if (connectedPlainAreas.Count >= 12) { break; }
					Area currentAreaBeingChecked = areasToCheck[0];
					//if area has already been reserved, skip it.
					if (!alreadyReservedAreas.Contains(currentAreaBeingChecked)) {
						if (currentAreaBeingChecked.elevationComponent.elevationType == ELEVATION.PLAIN) {
							//if the tile has at least 8 Tree Spots, tag it as Wood Source
							if (currentAreaBeingChecked.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT, TILE_OBJECT_TYPE.BIG_TREE_OBJECT, p_data) >= 8) {
								currentAreaBeingChecked.featureComponent.AddFeature(AreaFeatureDB.Wood_Source_Feature, currentAreaBeingChecked);
								lumberyardSpots++;
							}
							connectedPlainAreas.Add(currentAreaBeingChecked);
							//add neighbours as tile to be checked
							for (int i = 0; i < currentAreaBeingChecked.neighbourComponent.cardinalNeighbours.Count; i++) {
								Area neighbour = currentAreaBeingChecked.neighbourComponent.cardinalNeighbours[i];
								if (!checkedAreas.Contains(neighbour) && !areasToCheck.Contains(neighbour) && !alreadyReservedAreas.Contains(neighbour)) {
									areasToCheck.Add(neighbour);
								}
							}
						} else {
							// if (currentAreaBeingChecked.elevationComponent.elevationType == ELEVATION.WATER) {
							// 	if (!waterAreas.Contains(currentAreaBeingChecked) && waterAreas.Count < 2) {
							// 		//add fishing spot to area and tag as Fish Source
							// 		ReservedWaterAreaFishSourceHandling(p_data, currentAreaBeingChecked);
							// 		waterAreas.Add(currentAreaBeingChecked);
							// 	}
							// } else if (currentAreaBeingChecked.elevationComponent.elevationType == ELEVATION.MOUNTAIN) {
							// 	if (!caveAreas.Contains(currentAreaBeingChecked) && caveAreas.Count < 2) {
							// 		//add ore vein to area and tag as Metal Source
							// 		ReservedCaveAreaMetalSourceProcessing(p_data, currentAreaBeingChecked);
							// 		caveAreas.Add(currentAreaBeingChecked);
							// 		miningSpots++;
							// 	}
							// }
							villageSpotChoices.Remove(currentAreaBeingChecked);
						}	
						
						if (currentAreaBeingChecked.elevationComponent.HasElevation(ELEVATION.WATER)) {
							if (!waterAreas.Contains(currentAreaBeingChecked) && waterAreas.Count < 2) {
								//add fishing spot to area and tag as Fish Source
								ReservedWaterAreaFishSourceHandling(p_data, currentAreaBeingChecked);
								waterAreas.Add(currentAreaBeingChecked);
							}
						} 
						if (currentAreaBeingChecked.elevationComponent.HasElevation(ELEVATION.MOUNTAIN) && 
						    currentAreaBeingChecked.elevationComponent.elevationDictionary[ELEVATION.MOUNTAIN] > 5) {
							if (!caveAreas.Contains(currentAreaBeingChecked)) { //&& caveAreas.Count < 2 //Removed Count checking to maximize Ore Vein creation
								//add ore vein to area and tag as Metal Source
								ReservedCaveAreaMetalSourceProcessing(p_data, currentAreaBeingChecked, mainSpotCandidate);
								caveAreas.Add(currentAreaBeingChecked);
								miningSpots++;
							}
						}
					}
					areasToCheck.Remove(currentAreaBeingChecked);
					checkedAreas.Add(currentAreaBeingChecked);
				}

				if (connectedPlainAreas.Count >= 6) {
					VillageSpot villageSpot = p_data.AddVillageSpot(mainSpotCandidate, connectedPlainAreas, lumberyardSpots, miningSpots);
					//try to add more cave and water areas if less than 2
					if (waterAreas.Count < 2 || caveAreas.Count < 2) {
						for (int i = 0; i < villageSpot.reservedAreas.Count; i++) {
							if (waterAreas.Count >= 2 && caveAreas.Count >= 2) { break; }
							Area area = villageSpot.reservedAreas[i];
							for (int j = 0; j < area.neighbourComponent.cardinalNeighbours.Count; j++) {
								if (waterAreas.Count >= 2 && caveAreas.Count >= 2) { break; }
								Area neighbour = area.neighbourComponent.cardinalNeighbours[j];
								if (neighbour.elevationComponent.elevationType == ELEVATION.MOUNTAIN) {
									if (caveAreas.Count < 2 && !caveAreas.Contains(neighbour)) {
										//add ore vein to area and tag as Metal Source
										ReservedCaveAreaMetalSourceProcessing(p_data, neighbour, mainSpotCandidate);
										caveAreas.Add(neighbour);
									}
								} else if (neighbour.elevationComponent.elevationType == ELEVATION.WATER) {
									if (waterAreas.Count < 2 && !waterAreas.Contains(neighbour)) {
										//add fishing spot to area and tag as Fish Source
										ReservedWaterAreaFishSourceHandling(p_data, neighbour);
										waterAreas.Add(neighbour);
									}
								}
							}
						}
					}
					villageSpot.AddWaterAreas(waterAreas);
					villageSpot.AddCaveAreas(caveAreas);
					Debug.Log($"Created village spot at {mainSpotCandidate}. " +
					          $"\nAreas are({connectedPlainAreas.Count.ToString()}): {connectedPlainAreas.ComafyList()}" +
					          $"\nAdded Water areas({waterAreas.Count.ToString()}): {waterAreas.ComafyList()}" +
					          $"\nAdded Cave areas({caveAreas.Count.ToString()}): {caveAreas.ComafyList()}");
					
					//remove reserved areas from village spot choices, so they cannot be picked as a village spot.
					villageSpotChoices.ListRemoveRange(connectedPlainAreas);
					alreadyReservedAreas.AddRange(connectedPlainAreas);
					if (waterAreas.Count > 0) { alreadyReservedAreas.AddRange(waterAreas); }
					if (caveAreas.Count > 0) { alreadyReservedAreas.AddRange(caveAreas); }
					
					unreservedAreas.ListRemoveRange(connectedPlainAreas);
					unreservedAreas.ListRemoveRange(waterAreas);
					unreservedAreas.ListRemoveRange(caveAreas);
					// AdditionalResourceCreationForVillageSpots(villageSpot);
				}
				RuinarchListPool<Area>.Release(areasToCheck);
				RuinarchListPool<Area>.Release(checkedAreas);
				RuinarchListPool<Area>.Release(connectedPlainAreas);
				RuinarchListPool<Area>.Release(caveAreas);
				RuinarchListPool<Area>.Release(waterAreas);
			} else {
				villageSpotChoices.Remove(mainSpotCandidate);
			}
		}
		
		RuinarchListPool<Area>.Release(villageSpotChoices);
		p_data.SetReservedAreas(alreadyReservedAreas); //do not cleanup reserved list since it will still be used after this
		p_data.SetUnreservedAreas(unreservedAreas); //do not cleanup unreservedAreas list since it will still be used after this 
		
		GridMap.Instance.mainRegion.SetVillageSpots(p_data.villageSpots);
		
		Debug.Log($"Created {p_data.villageSpots.Count.ToString()} Village Spots");
		yield return null;
	}
	private void ReservedCaveAreaMetalSourceProcessing(MapGenerationData p_data, Area currentAreaBeingChecked, Area p_mainSpotCandidate) {
		if (!currentAreaBeingChecked.featureComponent.HasFeature(AreaFeatureDB.Metal_Source_Feature)) {
			currentAreaBeingChecked.featureComponent.AddFeature(AreaFeatureDB.Metal_Source_Feature, currentAreaBeingChecked);
		}
		// if (!currentAreaBeingChecked.tileObjectComponent.HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE.ORE_VEIN)) {
		// 	int oreVeinAmount = GameUtilities.RandomBetweenTwoNumbers(5, 7);
		// 	for (int i = 0; i < oreVeinAmount; i++) {
		// 		//add a Ore Vein to a random cave tile inside area.
		// 		// LocationGridTile oreVeinLocation = p_data.GetFirstUnoccupiedNonEdgeCaveTile(currentAreaBeingChecked, p_data);
		// 		LocationGridTile oreVeinLocation = p_data.GetFirstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot(currentAreaBeingChecked, p_data, p_mainSpotCandidate);
		// 		if (oreVeinLocation != null) {
		// 			p_data.SetGeneratedMapPerlinDetails(oreVeinLocation, TILE_OBJECT_TYPE.NONE);	
		// 			currentAreaBeingChecked.region.innerMap.CreateOreVein(oreVeinLocation);
		// 		}	
		// 	}
		// }
		if (!currentAreaBeingChecked.tileObjectComponent.HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE.ORE_VEIN)) {
			int oreVeinAmount = GameUtilities.RandomBetweenTwoNumbers(5, 7);
			int createdOreVeinAmount = 0;
			for (int i = 0; i < oreVeinAmount; i++) {
				//add a Ore Vein to a random cave tile inside area.
				// LocationGridTile oreVeinLocation = p_data.GetFirstUnoccupiedNonEdgeCaveTile(currentAreaBeingChecked, p_data);
				LocationGridTile oreVeinLocation = p_data.GetFirstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot(currentAreaBeingChecked, p_data, p_mainSpotCandidate);
				if (oreVeinLocation != null) {
					p_data.SetGeneratedMapPerlinDetails(oreVeinLocation, TILE_OBJECT_TYPE.NONE);
					currentAreaBeingChecked.region.innerMap.CreateOreVein(oreVeinLocation);
					createdOreVeinAmount++;
				}
			}
			if (createdOreVeinAmount <= 0) {
				for (int i = 0; i < 2; i++) {
					LocationGridTile oreVeinLocation = p_data.GetFirstUnoccupiedNonEdgeCaveTile(currentAreaBeingChecked, p_data);
					if (oreVeinLocation != null) {
						//fail-safe in case village spot is not placed in an optimal position, and no ore veins facing it could be placed.
						p_data.SetGeneratedMapPerlinDetails(oreVeinLocation, TILE_OBJECT_TYPE.NONE);	
						currentAreaBeingChecked.region.innerMap.CreateOreVein(oreVeinLocation);
					}	
				}
			}
		}

		
	}
	private void ReservedWaterAreaFishSourceHandling(MapGenerationData p_data, Area currentAreaBeingChecked) {
		if (!currentAreaBeingChecked.featureComponent.HasFeature(AreaFeatureDB.Fish_Source)) {
			currentAreaBeingChecked.featureComponent.AddFeature(AreaFeatureDB.Fish_Source, currentAreaBeingChecked);
			if (!currentAreaBeingChecked.tileObjectComponent.HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE.FISHING_SPOT)) {
				//add a fishing spot to a random ocean tile inside area.
				LocationGridTile fishingSpotLocation = p_data.GetFirstUnoccupiedNonEdgeOceanTile(currentAreaBeingChecked);
				currentAreaBeingChecked.region.innerMap.CreateFishingSpot(fishingSpotLocation);
			}
		}
	}
	private bool IsAreaValidVillageSpotCandidate(Area p_area, MapGenerationData p_mapGenerationData) {
		if (p_area.elevationComponent.elevationType == ELEVATION.PLAIN && p_area.elevationComponent.IsFully(ELEVATION.PLAIN)) {
			if (p_area.neighbourComponent.HasCardinalNeighbourWithElevationThatIsNotReservedByOtherVillage(ELEVATION.WATER, p_mapGenerationData.villageSpots) || 
			    p_area.neighbourComponent.HasCardinalNeighbourWithElevationThatIsNotReservedByOtherVillage(ELEVATION.MOUNTAIN, p_mapGenerationData.villageSpots)) {
				return true;
			}
		}
		return false;
	}
	private void AdditionalResourceCreation() {
		int randomResourceCount = 0;
		for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++) {
			randomResourceCount += UnityEngine.Random.Range(1, 4);
		}
		List<Area> areaChoices = RuinarchListPool<Area>.Claim();
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
			Area area = GridMap.Instance.allAreas[i];
			if (area.GetOccupyingVillageSpot() == null) {
				areaChoices.Add(area);
			}
		}
		
		//
		// areaChoices.AddRange(p_villageSpot.reservedAreas);
		//
		// areaChoices.Remove(p_villageSpot.mainSpot);
		// List<Area> areasAround = RuinarchListPool<Area>.Claim();
		// p_villageSpot.mainSpot.PopulateAreasInRange(areasAround, 2, true);
		// areaChoices.ListRemoveRange(areasAround);
		// RuinarchListPool<Area>.Release(areasAround);
		
		List<string> randomResourceChoices = RuinarchListPool<string>.Claim();
		randomResourceChoices.Add("BOAR_DEN");
		randomResourceChoices.Add("WOLF_DEN");
		randomResourceChoices.Add("BEAR_DEN");
        randomResourceChoices.Add("RABBIT_HOLE");
        randomResourceChoices.Add("Game Feature");
        randomResourceChoices.Add("MINK_HOLE");
        randomResourceChoices.Add("MOONCRAWLER_HOLE");

        if (areaChoices.Count > 0) {
			for (int i = 0; i < randomResourceCount; i++) {
				if (areaChoices.Count == 0) { break; }
				string randomType = CollectionUtilities.GetRandomElement(randomResourceChoices);
				Area randomArea = CollectionUtilities.GetRandomElement(areaChoices);

				if (randomType == "Game Feature") {
					randomArea.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, randomArea);
				} else {
					STRUCTURE_TYPE structureType = (STRUCTURE_TYPE) Enum.Parse(typeof(STRUCTURE_TYPE), randomType);
					GameObject structurePrefab = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(new StructureSetting(structureType, RESOURCE.NONE));
					
					List<LocationGridTile> unoccupiedTiles = RuinarchListPool<LocationGridTile>.Claim();
					for (int j = 0; j < randomArea.gridTileComponent.gridTiles.Count; j++) {
						LocationGridTile tile = randomArea.gridTileComponent.gridTiles[j];
						if (tile.structure is Wilderness && tile.tileObjectComponent.objHere == null && tile.IsPassable()) {
							List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(4, 4), tile); //had to check 4x4 so that dens will not be directly adjacent to other structures
							int invalidOverlap = overlappedTiles.Count(t => t.tileObjectComponent.objHere != null || t.structure.structureType != STRUCTURE_TYPE.WILDERNESS || t.IsAtEdgeOfMap() || !t.IsPassable());
							if (invalidOverlap <= 0) {
								unoccupiedTiles.Add(tile);	
							}
						}
					}
					if (unoccupiedTiles.Count > 0) {
						LocationGridTile randomLocation = CollectionUtilities.GetRandomElement(unoccupiedTiles);
						NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(randomArea.region, LOCATION_TYPE.DUNGEON, randomArea);
						LocationStructure structure =  LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(settlement, GridMap.Instance.mainRegion.innerMap, structurePrefab, randomLocation);
						// TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(structureType);
						// randomLocation.structure.AddPOI(tileObject, randomLocation);
						Debug.Log($"Added animal den - {randomType.ToString()} to {randomLocation.ToString()}");
					}
					RuinarchListPool<LocationGridTile>.Release(unoccupiedTiles);
				}
				areaChoices.Remove(randomArea);
			}	
		}
		RuinarchListPool<string>.Release(randomResourceChoices);
	}
	private bool TryAssignSettlementTiles(MapGenerationData data) {
		int createdVillages = 0;
		int startingVillagesToCreate = WorldSettings.Instance.worldSettingsData.factionSettings.GetCurrentTotalVillageCountBasedOnFactions();
		if (data.villageSpots.Count < startingVillagesToCreate) {
			//not enough village spots
			return false;
		}
		for (int i = 0; i < WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates.Count; i++) {
			FactionTemplate factionTemplate = WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates[i];
			for (int j = 0; j < factionTemplate.villageSettings.Count; j++) {
				if (data.villageSpots.Count == 0) {
					return false; //not enough village spots 
				}
				VillageSpot chosenSpot = null;
				List<VillageSpot> choices = RuinarchListPool<VillageSpot>.Claim();
				for (int k = 0; k < data.villageSpots.Count; k++) {
					VillageSpot spot = data.villageSpots[k];
					if (spot.CanAccommodateFaction(factionTemplate.factionType)) {
						choices.Add(spot);
					}
				}
				if (choices.Count > 0) {
					chosenSpot = CollectionUtilities.GetRandomElement(choices);
				} else {
					//if no preferred tiles are available, then just choose at random from available village spots
					// chosenSpot = CollectionUtilities.GetRandomElement(data.villageSpots);
					return false; //no valid village spot found
				}
				// if (j == 0) {
				// 	List<VillageSpot> choices = RuinarchListPool<VillageSpot>.Claim();
				// 	for (int k = 0; k < data.villageSpots.Count; k++) {
				// 		VillageSpot spot = data.villageSpots[k];
				// 		if (spot.CanAccommodateFaction(factionTemplate.factionType)) {
				// 			choices.Add(spot);
				// 		}
				// 	}
				// 	if (choices.Count > 0) {
				// 		chosenSpot = CollectionUtilities.GetRandomElement(choices);
				// 	} else {
				// 		//if no preferred tiles are available, then just choose at random from available village spots
				// 		// chosenSpot = CollectionUtilities.GetRandomElement(data.villageSpots);
				// 		return false; //no valid village spot found
				// 	}
				// 	
				// } else {
				// 	//if not first village pick a spot nearest to First Village
				// 	float nearestDistance = Mathf.Infinity;
				// 	Vector2 firstVillagePos = data.determinedVillages[factionTemplate][0].mainSpot.areaData.position;
				// 	for (int k = 0; k < data.villageSpots.Count; k++) {
				// 		VillageSpot villageSpot = data.villageSpots[k];
				// 		Vector2 directionToTarget = villageSpot.mainSpot.areaData.position - firstVillagePos;
				// 		float distance = directionToTarget.sqrMagnitude;
				// 		if (distance < nearestDistance) {
				// 			nearestDistance = distance;
				// 			chosenSpot = villageSpot;
				// 		}
				// 	}
				// }
				Assert.IsNotNull(chosenSpot, $"Could not find village spot for {factionTemplate.name}'s Village #{j.ToString()}");
				data.AddDeterminedVillage(factionTemplate, chosenSpot);
				data.RemoveVillageSpot(chosenSpot);
				createdVillages++;
			}
		}
		return createdVillages == startingVillagesToCreate;
	}
	#endregion

	#region Settlement Generation Utilities
	private List<Area> GetNeighbouringTiles(List<Area> tiles) {
		List<Area> neighbouringTiles = new List<Area>();
		for (int i = 0; i < tiles.Count; i++) {
			Area tile = tiles[i];
			for (int j = 0; j < tile.neighbourComponent.neighbours.Count; j++) {
				Area neighbour = tile.neighbourComponent.neighbours[j];
				if (tiles.Contains(neighbour) == false && neighbouringTiles.Contains(neighbour) == false) {
					neighbouringTiles.Add(neighbour);
				}
			}
		}
		return neighbouringTiles;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		SaveDataArea[,] savedMap = scenarioMapData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataArea savedHexTile = savedMap[x, y];
				Area hexTile = GridMap.Instance.map[x, y];
				if (savedHexTile.tileFeatureSaveData?.Count > 0) {
					for (int i = 0; i < savedHexTile.tileFeatureSaveData.Count; i++) {
						SaveDataAreaFeature saveDataTileFeature = savedHexTile.tileFeatureSaveData[i];
						AreaFeature tileFeature = saveDataTileFeature.Load();
						hexTile.featureComponent.AddFeature(tileFeature, hexTile);
					}
				}
				yield return null;
			}
		}
		List<VillageSpot> villageSpots = RuinarchListPool<VillageSpot>.Claim();
		for (int i = 0; i < scenarioMapData.worldMapSave.villageSpots.Count; i++) {
			SaveDataVillageSpot saveDataVillageSpot = scenarioMapData.worldMapSave.villageSpots[i];
			VillageSpot villageSpot = saveDataVillageSpot.Load();
			villageSpots.Add(villageSpot);
		}
		GridMap.Instance.mainRegion.SetVillageSpots(villageSpots);
		//Creation of animal dens is in a separate loop because we now have to link to a village spots
		//So, the region's list of village spots should be set first before creation of dens
  //      for (int i = 0; i < villageSpots.Count; i++) {
		//	AdditionalResourceCreationForVillageSpots(villageSpots[i]);
		//}
		RuinarchListPool<VillageSpot>.Release(villageSpots);
		// AdditionalResourceCreation();
	}
	private void DetermineSettlementsForTutorial() {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[6, 5],
			GridMap.Instance.map[7, 5],
			GridMap.Instance.map[6, 6],
			GridMap.Instance.map[5, 5],
		};
	
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
		}
		
		// List<Area> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		//if settlement is not adjacent to any water hex tile create one
		// if (neighbouringTiles.Any(h => h.elevationType == ELEVATION.WATER) == false) {
		// 	Area randomTile = CollectionUtilities.GetRandomElement(neighbouringTiles);
		// 	randomTile.featureComponent.RemoveAllFeatures(randomTile);
		// }
	}
	private void DetermineSettlementsForOona(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[6, 5],
		};

		FactionTemplate factionTemplate = new FactionTemplate(1);
		factionTemplate.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			data.AddDeterminedVillage(factionTemplate, new VillageSpot(chosenTile, 1, 1));
		}
	}
	private void DetermineSettlementsForIcalawa(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[9, 2],
		};

		FactionTemplate factionTemplate = new FactionTemplate(1);
		factionTemplate.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			data.AddDeterminedVillage(factionTemplate, new VillageSpot(chosenTile, 1, 1));
		}
	}
	private void DetermineSettlementsForPangatLoo(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[2, 3],
		};

		FactionTemplate factionTemplate = new FactionTemplate(1);
		factionTemplate.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			data.AddDeterminedVillage(factionTemplate, new VillageSpot(chosenTile, 1, 1));
		}
	}
	private void DetermineSettlementsForAffatt(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[1, 2],
			GridMap.Instance.map[3, 8],
			GridMap.Instance.map[8, 3],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(2);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0 || i == 1) {
				data.AddDeterminedVillage(factionTemplate1, new VillageSpot(chosenTile, 1, 1));
			} else {
				data.AddDeterminedVillage(factionTemplate2, new VillageSpot(chosenTile, 1, 1));
			}
		}
	}
	private void DetermineSettlementsForZenko(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			//region 1 (snow)
			GridMap.Instance.map[4, 8],
			//region 2 (grassland)
			GridMap.Instance.map[8, 2],
			// GridMap.Instance.map[7, 7],
			//region 3 (forest)
			GridMap.Instance.map[1, 2],
			// GridMap.Instance.map[3, 3],
			//region 4 (desert)
			GridMap.Instance.map[10, 8],
			// GridMap.Instance.map[9, 3],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(1);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate3 = new FactionTemplate(1);
		factionTemplate3.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate4 = new FactionTemplate(1);
		factionTemplate4.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0) {
				data.AddDeterminedVillage(factionTemplate1, new VillageSpot(chosenTile, 1, 1));
			} else if (i == 1) {
				data.AddDeterminedVillage(factionTemplate2, new VillageSpot(chosenTile, 1, 1));
			} else if (i == 2) {
				data.AddDeterminedVillage(factionTemplate3, new VillageSpot(chosenTile, 1, 1));
			} else {
				data.AddDeterminedVillage(factionTemplate4, new VillageSpot(chosenTile, 1, 1));
			}
		}
	}
	private void DetermineSettlementsForAneem(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[2, 5],
			GridMap.Instance.map[12, 2],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(1);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0) {
				data.AddDeterminedVillage(factionTemplate1, new VillageSpot(chosenTile, 1, 1));
			} else {
				data.AddDeterminedVillage(factionTemplate2, new VillageSpot(chosenTile, 1, 1));
			}
		}
	}
	private void DetermineSettlementsForPitto(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[4, 5],
			GridMap.Instance.map[8, 5],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(1);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0) {
				data.AddDeterminedVillage(factionTemplate1, new VillageSpot(chosenTile, 1, 1));
			} else {
				data.AddDeterminedVillage(factionTemplate2, new VillageSpot(chosenTile, 1, 1));
			}
		}
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataArea savedHexTile = savedMap[x, y];
				Area hexTile = GridMap.Instance.map[x, y];
				if (savedHexTile.tileFeatureSaveData?.Count > 0) {
					for (int i = 0; i < savedHexTile.tileFeatureSaveData.Count; i++) {
						SaveDataAreaFeature saveDataTileFeature = savedHexTile.tileFeatureSaveData[i];
						AreaFeature tileFeature = saveDataTileFeature.Load();
						hexTile.featureComponent.AddFeature(tileFeature, hexTile);
					}
				}
				yield return null;
			}
		}
	}
	#endregion
}
