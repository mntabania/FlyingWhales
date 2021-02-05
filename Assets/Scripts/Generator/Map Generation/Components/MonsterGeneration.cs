﻿using System.Collections;
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
		// yield return MapGenerator.Instance.StartCoroutine(RegionalMonsterGeneration());
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

	#region Helpers
	private void CreateMonster(SUMMON_TYPE summonType, BaseSettlement settlementOnTile, BaseLandmark monsterLair, LocationStructure monsterLairStructure, Faction faction = null) {
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.GetDefaultFactionForMonster(summonType), settlementOnTile, monsterLair.tileLocation.region, monsterLairStructure);
		LocationGridTile targetTile = CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles);
		CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);
		//summon.AddTerritory(monsterLair.tileLocation);
        // summon.MigrateHomeStructureTo(monsterLairStructure);
        //summon.ChangeHomeStructure(monsterLairStructure);

        //if (monsterLairStructure is LocationStructure homeStructure) {
        //	summon.MigrateHomeStructureTo(homeStructure);	
        //}
    }
    private Summon CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, string className = "", Faction faction = null, params HexTile[] territories) {
		var chosenTile = homeStructure != null ? CollectionUtilities.GetRandomElement(homeStructure.unoccupiedTiles) : CollectionUtilities.GetRandomElement(locationChoices);
		
		Assert.IsNotNull(chosenTile, $"Chosen tile for {summonType.ToString()} is null!");
		Assert.IsTrue(chosenTile.collectionOwner.isPartOfParentRegionMap, $"Chosen tile for {summonType.ToString()} is not part of the region map!");

		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.GetDefaultFactionForMonster(summonType), null, chosenTile.parentMap.region, className: className);
		CharacterManager.Instance.PlaceSummonInitially(summon, chosenTile);
		if (homeStructure != null) {
			summon.MigrateHomeStructureTo(homeStructure);	
		} else {
			summon.SetTerritory(chosenTile.collectionOwner.partOfHextile.hexTileOwner, false);
			if (territories != null) {
				for (int i = 0; i < territories.Length; i++) {
					HexTile territory = territories[i];
					summon.SetTerritory(territory, false);
				}
			}	
		}
		return summon;
	}
    private void CreateCharacter(RACE race, string className, GENDER gender, BaseSettlement settlementOnTile, LocationStructure structure, Faction faction = null) {
        Character character = CharacterManager.Instance.CreateNewCharacter(className, race, gender, faction ?? FactionManager.Instance.neutralFaction, settlementOnTile, settlementOnTile.region, structure);
        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(structure.passableTiles);
        character.CreateMarker();
        character.InitialCharacterPlacement(targetTile);
    }
    #endregion

    private IEnumerator RegionalMonsterGeneration() {
	    Region region = GridMap.Instance.allRegions.First();
		for (int i = 0; i < region.regionDivisionComponent.divisions.Count; i++) {
			RegionDivision regionDivision = region.regionDivisionComponent.divisions[i];
			if (region.regionFeatureComponent.HasFeature<TeemingFeature>()) {
				continue; //do not generate monsters in region wilderness if region is Teeming
			}
			List<LocationGridTile> locationChoices = new List<LocationGridTile>();
			regionDivision.tiles.Where(h => h.settlementOnTile == null && h.landmarkOnTile == null && (h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES) && h.HasOwnedSettlementNeighbour() == false).
				ToList().ForEach(h => locationChoices.AddRange(h.locationGridTiles));
			if (locationChoices.Count == 0) {
				Debug.LogWarning($"Could not find valid tiles to place monsters at {region.name}");
				continue;
			}

			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
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
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
				AneemRegionalMonsters(i, ref locationChoices);
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
				PittoRegionalMonsters(i, ref locationChoices);
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
						Summon summon = CreateMonster(SUMMON_TYPE.Skeleton, locationChoices, className: CollectionUtilities.GetRandomElement(randomClassChoices), faction: FactionManager.Instance.undeadFaction);
						locationChoices.Remove(summon.gridTileLocation);
						if (locationChoices.Count == 0) {
							Debug.LogWarning($"Ran out of grid tiles to place monsters at region {region.name}");
							break;
						}
					}
				} else {
					MonsterMigrationBiomeAtomizedData chosenMMonster = regionDivision.GetRandomMonsterFromFaunaList();
					int randomAmount = GameUtilities.RandomBetweenTwoNumbers(chosenMMonster.minRange, chosenMMonster.maxRange);;
					for (int k = 0; k < randomAmount; k++) {
						Summon summon = CreateMonster(chosenMMonster.monsterType, locationChoices);
						locationChoices.Remove(summon.gridTileLocation);
					}
					if (locationChoices.Count == 0) {
						Debug.LogWarning($"Ran out of grid tiles to place monsters at region {region.name}");
						break;
					}
					// //spawn monsters base on provided regional settings
					// MonsterGenerationSetting monsterGenerationSetting = WorldConfigManager.Instance.worldWideMonsterGenerationSetting;
					// WeightedDictionary<MonsterSetting> monsterChoices = monsterGenerationSetting.GetMonsterChoicesForBiome(regionDivision.coreTile.biomeType);
					// if (monsterChoices != null) {
					// 	MonsterSetting randomMonsterSetting = monsterChoices.PickRandomElementGivenWeights();
					// 	int randomAmount = Random.Range(3, 9);
					// 	for (int k = 0; k < randomAmount; k++) {
					// 		Summon summon = CreateMonster(randomMonsterSetting.monsterType, locationChoices);
					// 		locationChoices.Remove(summon.gridTileLocation);
					// 	}
					// 	if (locationChoices.Count == 0) {
					// 		Debug.LogWarning($"Ran out of grid tiles to place monsters at region {regionDivision.name}");
					// 		break;
					// 	}
					// 		
					// }	
				}
			}
			yield return null;
		}
	}
	private IEnumerator LandmarkMonsterGeneration() {
		// if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
		// 	//no landmark monsters in tutorial world
		// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
		// 	OonaLandmarkMonsterGeneration();
		// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
		// 	PangatLooLandmarkMonsterGeneration();
		// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
		// 	ZenkoLandmarkMonsterGeneration();
		// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
		// 	AneemLandmarkMonsterGeneration();
		// }  else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
		// 	PittoLandmarkMonsterGeneration();
		// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
		
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			PangatLooLandmarkMonsterGeneration();
		}
		List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
		List<LocationGridTile> locationChoices = new List<LocationGridTile>();
		for (int i = 0; i < allLandmarks.Count; i++) {
			BaseLandmark landmark = allLandmarks[i];
			if (landmark.specificLandmarkType != LANDMARK_TYPE.CAVE) {
				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo && landmark.specificLandmarkType == LANDMARK_TYPE.ANCIENT_GRAVEYARD) {
					continue; //do not spawn other monsters in ancient graveyard for Pangat Loo since there are already skeletons there.
				}
				RegionDivision regionDivision = landmark.tileLocation.regionDivision;
				LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
				if (structure is RuinedZoo) {
					continue; //skip
				}
				if (GameUtilities.RollChance(70)) {
					locationChoices.Clear();
					locationChoices.AddRange(structure.passableTiles);
					
                    MonsterMigrationBiomeAtomizedData chosenMMonster = regionDivision.GetRandomMonsterFromFaunaList();
                    int randomAmount = GameUtilities.RandomBetweenTwoNumbers(chosenMMonster.minRange, chosenMMonster.maxRange);;
                    for (int k = 0; k < randomAmount; k++) {
	                    Summon summon = CreateMonster(chosenMMonster.monsterType, locationChoices, structure, faction: FactionManager.Instance.GetDefaultFactionForMonster(chosenMMonster.monsterType));
	                    if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
		                    summon.traitContainer.AddTrait(summon, "Mighty");
	                    }
	                    locationChoices.Remove(summon.gridTileLocation);
                    }
                    if (locationChoices.Count == 0) {
	                    Debug.LogWarning($"Ran out of grid tiles to place monsters at structure {structure.name}");
	                    break;
                    }
                }
			}
		}	
		yield return null;
	}
	private IEnumerator CaveMonsterGeneration() {
		Region region = GridMap.Instance.allRegions.First();
		if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
			List<LocationStructure> caves = region.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.CAVE);
			caves = caves.OrderByDescending(x => x.tiles.Count).ToList();
			
			// if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			// 	TutorialCaveMonsterGeneration(caves);
			// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			// 	OonaCaveMonsterGeneration(caves);
			// } else 
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
				IcalawaCaveMonsterGeneration(caves);
			} 
			// else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			// 	PangatLooCaveMonsterGeneration(caves);
			// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
			// 	AneemCaveMonsterGeneration(caves);
			// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
			// 	PittoCaveMonsterGeneration(caves);
			// } 
			else {
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
									CreateMonster(SUMMON_TYPE.Skeleton, cave.unoccupiedTiles.ToList(), className: CollectionUtilities.GetRandomElement(randomClassChoices), faction: FactionManager.Instance.undeadFaction);
								}	
							}
						}
						
					}
				} else {
					// WeightedDictionary<MonsterSetting> monsterChoices = caveData.monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
					List<LocationGridTile> locationChoices = new List<LocationGridTile>();
					for (int j = 0; j < caves.Count; j++) {
						LocationStructure cave = caves[j];
						if (cave.residents.Count > 0) {
							//if cave already has occupants, then do not generate monsters for that cave
							continue;
						}
						RegionDivision regionDivision = cave.occupiedHexTile.hexTileOwner.regionDivision;
						if (GameUtilities.RollChance(70)) {
							locationChoices.Clear();
							locationChoices.AddRange(cave.passableTiles);
					
							MonsterMigrationBiomeAtomizedData chosenMMonster = regionDivision.GetRandomMonsterFromFaunaList();
							int randomAmount = GameUtilities.RandomBetweenTwoNumbers(chosenMMonster.minRange, chosenMMonster.maxRange);;
							for (int k = 0; k < randomAmount; k++) {
								Summon summon = CreateMonster(chosenMMonster.monsterType, locationChoices, cave, faction: FactionManager.Instance.GetDefaultFactionForMonster(chosenMMonster.monsterType));
								if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
									summon.traitContainer.AddTrait(summon, "Mighty");
								}
								locationChoices.Remove(summon.gridTileLocation);
							}
							if (locationChoices.Count == 0) {
								Debug.LogWarning($"Ran out of grid tiles to place monsters at structure {cave.name}");
								break;
							}
						}
						// if (GenerateRatmen(cave, GameUtilities.RandomBetweenTwoNumbers(1, 3))) {
						//     //Ratmen has bee generated
						// } else {
						//     if (GameUtilities.RollChance(caveData.monsterGenerationChance)) {
						//         MonsterSetting randomMonsterSetting = monsterChoices.PickRandomElementGivenWeights();
						//         int randomAmount = randomMonsterSetting.minMaxRange.Random();
						//         for (int l = 0; l < randomAmount; l++) {
						//             CreateMonster(randomMonsterSetting.monsterType, cave.unoccupiedTiles.ToList(), cave);
						//         }
						//     }
						// }
					}	
				}
					
			}
		}
		yield return null;
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

	#region Tutorial
	private void TutorialCaveMonsterGeneration(List<LocationStructure> caves) {
		for (int j = 0; j < caves.Count; j++) {
			LocationStructure cave = caves[j];
			List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
			if (j == 0 || j == 1) {
				//Giant spiders	
				int randomGiantSpider = Random.Range(2, 5);
				for (int k = 0; k < randomGiantSpider; k++) {
					CreateMonster(SUMMON_TYPE.Giant_Spider, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
				}
			}
			else if (j == 2) {
				//Golem	
				int randomGolem = Random.Range(3, 6);
				for (int k = 0; k < randomGolem; k++) {
					CreateMonster(SUMMON_TYPE.Golem, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
				}
			}
			else {
				break;
			}
		}
	}
	#endregion
	
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
	private void OonaCaveMonsterGeneration(List<LocationStructure> caves) {
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
			}
			else if (j == 2) {
				//Fire Elementals	
				int fireElementals = 2;
				for (int k = 0; k < fireElementals; k++) {
					CreateMonster(SUMMON_TYPE.Fire_Elemental, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
				}
			}
			else {
				break;
			}
		}
	}
	#endregion

	#region Icalawa
	private void IcalawaCaveMonsterGeneration(List<LocationStructure> caves) {
		List<LocationStructure> shuffledCaves = CollectionUtilities.Shuffle(caves);
		if (shuffledCaves.Count > 0) {
			LocationStructure ratmenCave = shuffledCaves[0];
			shuffledCaves.Remove(ratmenCave);
			GenerateRatmen(ratmenCave, 3, 100);
		}
		
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
			int randomAmount = 2;
			for (int k = 0; k < randomAmount; k++) {
				CreateMonster(SUMMON_TYPE.Skeleton, landmark.tileLocation.settlementOnTile, landmark, structure, FactionManager.Instance.undeadFaction);
			}
		}
	}
	private void PangatLooCaveMonsterGeneration(List<LocationStructure> caves) {
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
			}
			else {
				break;
			}
		}
	}
    #endregion

    #region Ratmen Generation
    private bool GenerateRatmen(LocationStructure structure, int amount, int chance = 10) {
        if (GameUtilities.RollChance(chance)) {
            if (FactionManager.Instance.ratmenFaction == null) {
                //Only create ratmen faction if ratmen are spawned
                FactionManager.Instance.CreateRatmenFaction();
            }
            int numOfRatmen = amount;
            for (int k = 0; k < numOfRatmen; k++) {
                CreateCharacter(RACE.RATMAN, "Ratman", GENDER.MALE, structure.settlementLocation, structure, FactionManager.Instance.ratmenFaction);
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Aneem Generation
    private void AneemRegionalMonsters(int regionIndex, ref List<LocationGridTile> locationChoices) {
	    if (regionIndex == 0) {
		    //Sludge
		    int randomAmount = 8;
		    for (int k = 0; k < randomAmount; k++) {
			    if (locationChoices.Count == 0) { break; }
			    Summon summon = CreateMonster(SUMMON_TYPE.Sludge, locationChoices);
			    locationChoices.Remove(summon.gridTileLocation);
		    }
	    } else if (regionIndex == 1) {
		    //Fire Elemental
		    int randomAmount = 5;
		    for (int k = 0; k < randomAmount; k++) {
			    if (locationChoices.Count == 0) { break; }
			    Summon summon = CreateMonster(SUMMON_TYPE.Fire_Elemental, locationChoices);
			    locationChoices.Remove(summon.gridTileLocation);
		    }
	    }
    }
    private void AneemLandmarkMonsterGeneration() {
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
	    List<BaseLandmark> mageTowers = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MAGE_TOWER);
	    for (int i = 0; i < mageTowers.Count; i++) {
		    BaseLandmark landmark = mageTowers[i];
		    if (i == 0) {
			    //Golems
			    LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
			    int randomAmount = 4;
			    for (int k = 0; k < randomAmount; k++) {
				    CreateMonster(SUMMON_TYPE.Golem, landmark.tileLocation.settlementOnTile, landmark, structure, FactionManager.Instance.neutralFaction);
			    }
		    }
	    }
    }
    private void AneemCaveMonsterGeneration(List<LocationStructure> caves) {
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
		    }
		    else {
			    break;
		    }
	    }
    }
    #endregion

    #region Pitto
    private void PittoRegionalMonsters(int regionIndex, ref List<LocationGridTile> locationChoices) {
	    if (regionIndex == 0) {
		    //nymphs
		    int randomAmount = 12;
		    SUMMON_TYPE[] nymphTypes = new[] {SUMMON_TYPE.Ice_Nymph, SUMMON_TYPE.Water_Nymph, SUMMON_TYPE.Wind_Nymph};
		    for (int k = 0; k < randomAmount; k++) {
			    if (locationChoices.Count == 0) { break; }
			    Summon summon = CreateMonster(CollectionUtilities.GetRandomElement(nymphTypes), locationChoices);
			    locationChoices.Remove(summon.gridTileLocation);
		    }
		    //spawn 4-8 ghosts
		    int ghosts = Random.Range(4, 9);
		    for (int j = 0; j < ghosts; j++) {
			    if (locationChoices.Count == 0) { break; }
			    Summon summon = CreateMonster(SUMMON_TYPE.Ghost, locationChoices);
			    locationChoices.Remove(summon.gridTileLocation);
		    }
	    }
    }
    private void PittoLandmarkMonsterGeneration() {
	    List<BaseLandmark> graveyards = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.ANCIENT_GRAVEYARD);
	    for (int i = 0; i < graveyards.Count; i++) {
		    BaseLandmark landmark = graveyards[i];
		    LocationStructure graveyard = landmark.tileLocation.GetMostImportantStructureOnTile();
		    List<LocationGridTile> locationChoices = new List<LocationGridTile>(graveyard.tiles); 
		    if (i == 0) {
			    //spawn 4-8 to Skeletons
			    List<string> randomClassChoices = CharacterManager.Instance.GetNormalCombatantClasses().Select(x => x.className).ToList();
			    int skeletons = Random.Range(4, 9);
			    for (int j = 0; j < skeletons; j++) {
				    if (locationChoices.Count == 0) { break; }
				    Summon summon = CreateMonster(SUMMON_TYPE.Skeleton, locationChoices, graveyard, className: CollectionUtilities.GetRandomElement(randomClassChoices), faction: FactionManager.Instance.undeadFaction);
				    locationChoices.Remove(summon.gridTileLocation);
			    }
		    }
	    }
	    List<BaseLandmark> monsterLairs = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
	    for (int i = 0; i < monsterLairs.Count; i++) {
		    BaseLandmark landmark = monsterLairs[i];
		    //Wolves
		    LocationStructure monsterLair = landmark.tileLocation.GetMostImportantStructureOnTile();
		    int randomAmount = 4;
		    for (int k = 0; k < randomAmount; k++) {
			    CreateMonster(SUMMON_TYPE.Wolf, landmark.tileLocation.settlementOnTile, landmark, monsterLair, FactionManager.Instance.neutralFaction);
		    }
	    }
    }
    private void PittoCaveMonsterGeneration(List<LocationStructure> caves) {
	    int cavesWithWurms = 0;
	    for (int j = 0; j < caves.Count; j++) {
		    LocationStructure cave = caves[j];
		    if (cave.residents.Count > 0) {
			    //if cave already has occupants, then do not generate monsters for that cave
			    continue;
		    }
		    if (cavesWithWurms >= 3) {
			    break;
		    }
		    List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
		    int randomWurms = Random.Range(5, 9);
		    for (int k = 0; k < randomWurms; k++) {
			    CreateMonster(SUMMON_TYPE.Wurm, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
		    }
		    cavesWithWurms++;

	    }
    }
    #endregion
}
