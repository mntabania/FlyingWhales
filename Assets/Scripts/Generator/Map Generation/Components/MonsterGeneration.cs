using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Region_Features;
using Locations.Settlements;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class MonsterGeneration : MapGenerationComponent {

	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Placing monsters...");
		yield return MapGenerator.Instance.StartCoroutine(RegionalMonsterGeneration());
		yield return MapGenerator.Instance.StartCoroutine(LandmarkMonsterGeneration());
		yield return MapGenerator.Instance.StartCoroutine(CaveMonsterGeneration());
		yield return null;
	}

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			yield return null; //no monsters in affatt
		} else {
			yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));	
		}
	}
	#endregion
	
	// #region Saved World
	// public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
	// 	yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	// }
	// #endregion

	#region Helpers
	private void CreateMonster(SUMMON_TYPE summonType, BaseSettlement settlementOnTile, BaseLandmark monsterLair, LocationStructure monsterLairStructure, Faction faction = null) {
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.neutralFaction, settlementOnTile, monsterLair.tileLocation.region, monsterLairStructure);
		LocationGridTile targetTile = CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles);
		CharacterManager.Instance.PlaceSummon(summon, targetTile);
		//summon.AddTerritory(monsterLair.tileLocation);
        // summon.MigrateHomeStructureTo(monsterLairStructure);
        //summon.ChangeHomeStructure(monsterLairStructure);

        //if (monsterLairStructure is LocationStructure homeStructure) {
        //	summon.MigrateHomeStructureTo(homeStructure);	
        //}
    }
    private Summon CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, string className = "", Faction faction = null, params HexTile[] territories) {
		var chosenTile = homeStructure != null ? 
			CollectionUtilities.GetRandomElement(homeStructure.unoccupiedTiles) : 
			CollectionUtilities.GetRandomElement(locationChoices);
		
		Assert.IsNotNull(chosenTile, $"Chosen tile for {summonType.ToString()} is null!");
		Assert.IsTrue(chosenTile.collectionOwner.isPartOfParentRegionMap, $"Chosen tile for {summonType.ToString()} is not part of the region map!");
		
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.neutralFaction, null, chosenTile.parentMap.region, className: className);
		CharacterManager.Instance.PlaceSummon(summon, chosenTile);
		if (homeStructure != null) {
			summon.MigrateHomeStructureTo(homeStructure);	
		} else {
			summon.AddTerritory(chosenTile.collectionOwner.partOfHextile.hexTileOwner, false);
			if (territories != null) {
				for (int i = 0; i < territories.Length; i++) {
					HexTile territory = territories[i];
					summon.AddTerritory(territory, false);
				}
			}	
		}


		//summon.ChangeHomeStructure(homeStructure);
  //      if (homeStructure is IDwelling structure) {
		//	summon.MigrateHomeStructureTo(structure);
		//}
		return summon;
	}
	#endregion
	

	private IEnumerator RegionalMonsterGeneration() {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.regionFeatureComponent.HasFeature<TeemingFeature>()) {
				continue; //do not generate monsters in region wilderness if region is Teeming
			}
			List<LocationGridTile> locationChoices = new List<LocationGridTile>();
			region.tiles.Where(h => h.landmarkOnTile == null && (h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES) && h.HasOwnedSettlementNeighbour() == false).
				ToList().ForEach(h => locationChoices.AddRange(h.locationGridTiles));
			if (locationChoices.Count == 0) {
				Debug.LogWarning($"Could not find valid tiles to place monsters at {region.name}");
				continue;
			}

			if (WorldConfigManager.Instance.isTutorialWorld) {
				//sludge
				int randomSludge = 3;
				for (int k = 0; k < randomSludge; k++) {
					if (locationChoices.Count == 0) { break; }
					Summon summon = CreateMonster(SUMMON_TYPE.Sludge, locationChoices);
					locationChoices.Remove(summon.gridTileLocation);
				}
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
				//Succubus
				int randomSuccubus = 3;
				for (int k = 0; k < randomSuccubus; k++) {
					if (locationChoices.Count == 0) { break; }
					Summon summon = CreateMonster(SUMMON_TYPE.Succubus, locationChoices);
					locationChoices.Remove(summon.gridTileLocation);
				}
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
				ZenkoRegionalMonsters(i, ref locationChoices);
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
				PangatLooRegionalMonsters(i, ref locationChoices);
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
				if (region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
					//spawn 4-8 ghosts
					int ghosts = Random.Range(4, 9);
					for (int j = 0; j < ghosts; j++) {
						Summon summon = CreateMonster(SUMMON_TYPE.Ghost, locationChoices);
						locationChoices.Remove(summon.gridTileLocation);
						if (locationChoices.Count == 0) {
							Debug.LogWarning($"Ran out of grid tiles to place monsters at region {region.name}");
							break;
						}
					}
					//spawn 4-8 to Skeletons
					List<string> randomClassChoices = CharacterManager.Instance.GetNormalCombatantClasses().Select(x => x.className).ToList();
					int skeletons = Random.Range(4, 9);
					for (int j = 0; j < skeletons; j++) {
						Summon summon = CreateMonster(SUMMON_TYPE.Skeleton, locationChoices, 
							className: CollectionUtilities.GetRandomElement(randomClassChoices));
						locationChoices.Remove(summon.gridTileLocation);
						if (locationChoices.Count == 0) {
							Debug.LogWarning($"Ran out of grid tiles to place monsters at region {region.name}");
							break;
						}
					}
				} else {
					//spawn monsters base on provided regional settings
					MonsterGenerationSetting monsterGenerationSetting = WorldConfigManager.Instance.worldWideMonsterGenerationSetting;
					WeightedDictionary<MonsterSetting> monsterChoices = monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
					if (monsterChoices != null) {
						MonsterSetting randomMonsterSetting = monsterChoices.PickRandomElementGivenWeights();
						int randomAmount = Random.Range(3, 9);
						for (int k = 0; k < randomAmount; k++) {
							Summon summon = CreateMonster(randomMonsterSetting.monsterType, locationChoices);
							locationChoices.Remove(summon.gridTileLocation);
						}
						if (locationChoices.Count == 0) {
							Debug.LogWarning($"Ran out of grid tiles to place monsters at region {region.name}");
							break;
						}
							
					}	
				}
			}
			yield return null;
		}
	}
	private IEnumerator LandmarkMonsterGeneration() {
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			//no landmark monsters in tutorial world
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			OonaLandmarkMonsterGeneration();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			PangatLooLandmarkMonsterGeneration();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			ZenkoLandmarkMonsterGeneration();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
			List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
			for (int i = 0; i < allLandmarks.Count; i++) {
				BaseLandmark landmark = allLandmarks[i];
				if (landmark.specificLandmarkType != LANDMARK_TYPE.CAVE) {
					LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
					LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmark.specificLandmarkType);
					if (landmarkData.monsterGenerationSetting != null) {
						WeightedDictionary<MonsterSetting> monsterChoices = landmarkData.monsterGenerationSetting.GetMonsterChoicesForBiome(landmark.tileLocation.biomeType);
						if (monsterChoices != null && GameUtilities.RollChance(landmarkData.monsterGenerationChance)) {
							MonsterSetting randomMonsterSetting = monsterChoices.PickRandomElementGivenWeights();
							int randomAmount = randomMonsterSetting.minMaxRange.Random();
							for (int k = 0; k < randomAmount; k++) {
								CreateMonster(randomMonsterSetting.monsterType, landmark.tileLocation.settlementOnTile, landmark, structure);	
							}
							yield return null;
						}
					}
				}
			}	
		}
	}
	private IEnumerator CaveMonsterGeneration() {
		LandmarkData caveData = LandmarkManager.Instance.GetLandmarkData(LANDMARK_TYPE.CAVE);
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
				List<LocationStructure> caves = region.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.CAVE);
				caves = caves.OrderByDescending(x => x.tiles.Count).ToList();
				
				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
					for (int j = 0; j < caves.Count; j++) {
						LocationStructure cave = caves[j];
						List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
						if (j == 0 || j == 1) {
							//Giant spiders	
							int randomGiantSpider = Random.Range(2, 5);
							for (int k = 0; k < randomGiantSpider; k++) {
								CreateMonster(SUMMON_TYPE.Giant_Spider, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
							}
						} else if (j == 2) {
							//Golem	
							int randomGolem = Random.Range(3, 6);
							for (int k = 0; k < randomGolem; k++) {
								CreateMonster(SUMMON_TYPE.Golem, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
							}
						} else {
							break;
						}
					}
				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
					for (int j = 0; j < caves.Count; j++) {
						LocationStructure cave = caves[j];
						if (cave.residents.Count > 0) {
							//if cave already has occupants, then do not generate monsters for that cave
							continue;
						}
						List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
						if (j == 0 || j == 1) {
							//Trolls	
							int randomTrolls = Random.Range(3, 6);
							for (int k = 0; k < randomTrolls; k++) {
								CreateMonster(SUMMON_TYPE.Troll, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
							}
						} else if (j == 2) {
							//Fire Elementals	
							int fireElementals = 2;
							for (int k = 0; k < fireElementals; k++) {
								CreateMonster(SUMMON_TYPE.Fire_Elemental, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
							}
						} else {
							break;
						}
					}
				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
					List<LocationStructure> shuffledCaves = CollectionUtilities.Shuffle(caves);
					for (int j = 0; j < shuffledCaves.Count; j++) {
						LocationStructure cave = shuffledCaves[j];
						if (cave.residents.Count > 0) {
							//if cave already has occupants, then do not generate monsters for that cave
							continue;
						}
						List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
						if (j < 4) {
							CreateMonster(SUMMON_TYPE.Wurm, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
						}
					}
				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
					for (int j = 0; j < caves.Count; j++) {
						LocationStructure cave = caves[j];
						if (cave.residents.Count > 0) {
							//if cave already has occupants, then do not generate monsters for that cave
							continue;
						}
						List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
						if (j == 0) {
							for (int k = 0; k < 8; k++) {
								CreateMonster(SUMMON_TYPE.Wurm, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());	
							}
						} else {
							break;
						}
					}
				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
					if (region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
						for (int j = 0; j < caves.Count; j++) {
							if (GameUtilities.RollChance(40)) {
								LocationStructure cave = caves[j];
								if (GameUtilities.RollChance(50)) {
									//spawn 2-4 ghosts
									int ghosts = Random.Range(2, 5);
									for (int k = 0; k < ghosts; k++) {
										CreateMonster(SUMMON_TYPE.Ghost, cave.unoccupiedTiles.ToList());
									}	
								} else {
									//spawn 2-4 to Skeletons
									List<string> randomClassChoices = CharacterManager.Instance.GetNormalCombatantClasses().Select(x => x.className).ToList();
									int skeletons = Random.Range(2, 5);
									for (int k = 0; k < skeletons; k++) {
										CreateMonster(SUMMON_TYPE.Skeleton, cave.unoccupiedTiles.ToList(), 
											className: CollectionUtilities.GetRandomElement(randomClassChoices));
									}	
								}
							}
							
						}
					} else {
						WeightedDictionary<MonsterSetting> monsterChoices = caveData.monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
						for (int j = 0; j < caves.Count; j++) {
							LocationStructure cave = caves[j];
							if (cave.residents.Count > 0) {
								//if cave already has occupants, then do not generate monsters for that cave
								continue;
							}
							if (GameUtilities.RollChance(caveData.monsterGenerationChance)) {
								MonsterSetting randomMonsterSetting = monsterChoices.PickRandomElementGivenWeights();
								int randomAmount = randomMonsterSetting.minMaxRange.Random();
								for (int l = 0; l < randomAmount; l++) {
									CreateMonster(randomMonsterSetting.monsterType, cave.unoccupiedTiles.ToList(), cave);	
								}	
							}
						}	
					}
						
				}
			}
			yield return null;
		}
	}

	private List<HexTile> GetHexTileCountOfCave(LocationStructure caveStructure) {
		List<HexTile> tiles = new List<HexTile>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++) {
			LocationGridTile tile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (tile.collectionOwner.isPartOfParentRegionMap && tiles.Contains(tile.collectionOwner.partOfHextile.hexTileOwner) == false) {
				tiles.Add(tile.collectionOwner.partOfHextile.hexTileOwner);
			}
		}
		return tiles;
	}

	#region Zenko
	private void ZenkoRegionalMonsters(int regionIndex, ref List<LocationGridTile> locationChoices) {
		if (regionIndex == 0) {
			//nymphs
			int randomAmount = 8;
			SUMMON_TYPE[] nymphTypes = new[] {SUMMON_TYPE.Ice_Nymph, SUMMON_TYPE.Water_Nymph, SUMMON_TYPE.Wind_Nymph};
			for (int k = 0; k < randomAmount; k++) {
				if (locationChoices.Count == 0) { break; }
				Summon summon = CreateMonster(CollectionUtilities.GetRandomElement(nymphTypes), locationChoices);
				locationChoices.Remove(summon.gridTileLocation);
			}
		} else if (regionIndex == 1) {
			//fire elementals
			int randomAmount = 5;
			for (int k = 0; k < randomAmount; k++) {
				if (locationChoices.Count == 0) { break; }
				Summon summon = CreateMonster(SUMMON_TYPE.Fire_Elemental, locationChoices);
				locationChoices.Remove(summon.gridTileLocation);
			}
		} else if (regionIndex == 2) {
			//Kobolds
			int randomAmount = 5;
			for (int k = 0; k < randomAmount; k++) {
				if (locationChoices.Count == 0) { break; }
				Summon summon = CreateMonster(SUMMON_TYPE.Kobold, locationChoices);
				locationChoices.Remove(summon.gridTileLocation);
			}
		}
	}
	private void ZenkoLandmarkMonsterGeneration() {
		List<BaseLandmark> lairs = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
		for (int i = 0; i < lairs.Count; i++) {
			BaseLandmark landmark = lairs[i];
			if (i == 0) {
				//Kobolds
				LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
				int randomAmount = 4;
				for (int k = 0; k < randomAmount; k++) {
					CreateMonster(SUMMON_TYPE.Kobold, landmark.tileLocation.settlementOnTile, landmark, structure, FactionManager.Instance.neutralFaction);
				}
			} else if (i == 1) {
				LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
				int randomAmount = 3;
				for (int k = 0; k < randomAmount; k++) {
					CreateMonster(SUMMON_TYPE.Giant_Spider, landmark.tileLocation.settlementOnTile, landmark, structure, FactionManager.Instance.neutralFaction);
				}
			}
		}
	}
	#endregion

	#region Oona
	private void OonaLandmarkMonsterGeneration() {
		//wolves at monster lair
		List<BaseLandmark> monsterLairs = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
		for (int i = 0; i < monsterLairs.Count; i++) {
			BaseLandmark landmark = monsterLairs[i];
			LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
			int randomAmount = 3;
			for (int k = 0; k < randomAmount; k++) {
				CreateMonster(SUMMON_TYPE.Wolf, landmark.tileLocation.settlementOnTile, landmark, structure);
			}
		}
		//Spiders at Temple
		List<BaseLandmark> temples = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.TEMPLE);
		for (int i = 0; i < temples.Count; i++) {
			BaseLandmark landmark = temples[i];
			LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
			// int randomAmount = 2;
			// for (int k = 0; k < randomAmount; k++) {
			// 	CreateMonster(SUMMON_TYPE.Kobold, landmark.tileLocation.settlementOnTile, landmark, structure);	
			// }
			//Giant spiders	
			int randomGiantSpider = Random.Range(2, 5);
			for (int k = 0; k < randomGiantSpider; k++) {
				CreateMonster(SUMMON_TYPE.Giant_Spider, landmark.tileLocation.settlementOnTile, landmark, structure);
			}
		}
	}
	#endregion
	
	#region Pangat Loo
	private void PangatLooRegionalMonsters(int regionIndex, ref List<LocationGridTile> locationChoices) {
		if (regionIndex == 1) {
			//ghosts
			int randomAmount = 8;
			for (int k = 0; k < randomAmount; k++) {
				if (locationChoices.Count == 0) { break; }
				Summon summon = CreateMonster(SUMMON_TYPE.Ghost, locationChoices, faction: FactionManager.Instance.undeadFaction);
				locationChoices.Remove(summon.gridTileLocation);
			}
		}
	}
	private void PangatLooLandmarkMonsterGeneration() {
		//skeletons at ancient graveyard
		List<BaseLandmark> graveyards = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.ANCIENT_GRAVEYARD);
		for (int i = 0; i < graveyards.Count; i++) {
			BaseLandmark landmark = graveyards[i];
			LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
			int randomAmount = 3;
			for (int k = 0; k < randomAmount; k++) {
				CreateMonster(SUMMON_TYPE.Skeleton, landmark.tileLocation.settlementOnTile, landmark, structure, FactionManager.Instance.undeadFaction);
			}
		}
		// //Wolves at Monster Lair
		// List<BaseLandmark> monsterLair = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
		// for (int i = 0; i < monsterLair.Count; i++) {
		// 	BaseLandmark landmark = monsterLair[i];
		// 	LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
		// 	int randomAmount = Random.Range(2, 5);
		// 	for (int k = 0; k < randomAmount; k++) {
		// 		CreateMonster(SUMMON_TYPE.Wolf, landmark.tileLocation.settlementOnTile, landmark, structure);
		// 	}
		// }
	}
	#endregion
}
