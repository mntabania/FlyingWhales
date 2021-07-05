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
		LevelLoaderManager.Instance.UpdateLoadingInfo("Placing Monsters...");
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
	private void CreateMonster(SUMMON_TYPE summonType, BaseSettlement settlementOnTile, Region region, LocationStructure monsterLairStructure, Faction faction = null) {
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.GetDefaultFactionForMonster(summonType), settlementOnTile, region, monsterLairStructure);
		LocationGridTile targetTile = CollectionUtilities.GetRandomElement(monsterLairStructure.passableTiles);
		CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);
	}
    private Summon CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, string className = "", Faction faction = null) {
		var chosenTile = homeStructure != null ? CollectionUtilities.GetRandomElement(homeStructure.passableTiles) : CollectionUtilities.GetRandomElement(locationChoices);
		
		Assert.IsNotNull(chosenTile, $"Chosen tile for {summonType.ToString()} is null!");

		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.GetDefaultFactionForMonster(summonType), null, chosenTile.parentMap.region, className: className);
		CharacterManager.Instance.PlaceSummonInitially(summon, chosenTile);
		if (homeStructure != null) {
			summon.MigrateHomeStructureTo(homeStructure);	
		} else {
            if (chosenTile.structure != null && chosenTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && chosenTile.structure.structureType != STRUCTURE_TYPE.OCEAN) {
				summon.MigrateHomeStructureTo(chosenTile.structure);
			} else {
				summon.SetTerritory(chosenTile.area, false);
			}
			//Why set multiple territories here? Character cannot have multiple territories, this will only override the existing territory
			//if (territories != null) {
			//	for (int i = 0; i < territories.Length; i++) {
			//		Area territory = territories[i];
			//		summon.SetTerritory(territory, false);
			//	}
			//}	
		}
		return summon;
	}
    #endregion

    private IEnumerator LandmarkMonsterGeneration() {
	    if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			PangatLooLandmarkMonsterGeneration();
		}
		List<LocationStructure> allSpecialStructures = RuinarchListPool<LocationStructure>.Claim();
		LandmarkManager.Instance.PopulateAllSpecialStructures(allSpecialStructures);
		List<LocationGridTile> locationChoices = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < allSpecialStructures.Count; i++) {
			LocationStructure structure = allSpecialStructures[i];
			if (structure.structureType != STRUCTURE_TYPE.CAVE && !(structure is AnimalDen)) {
				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo && structure.structureType == STRUCTURE_TYPE.ANCIENT_GRAVEYARD) {
					continue; //do not spawn other monsters in ancient graveyard for Pangat Loo since there are already skeletons there.
				}
				Assert.IsNotNull(structure.occupiedArea, $"Occupied area of {structure.name} is null!");
				Assert.IsTrue(structure.tiles.Count > 0, $"{structure.name} has no tiles!");
				BiomeDivision biomeDivision = structure.region.biomeDivisionComponent.GetBiomeDivisionThatTileBelongsTo(structure.tiles.First());
				if (structure is RuinedZoo) {
					continue; //skip
				}
				if (GameUtilities.RollChance(70)) {
					locationChoices.Clear();
					locationChoices.AddRange(structure.passableTiles);
					
                    MonsterMigrationBiomeAtomizedData chosenMMonster = biomeDivision.GetRandomMonsterFromFaunaList();
                    if (chosenMMonster.monsterType == SUMMON_TYPE.Fire_Elemental) {
	                    continue; //temporarily disabled fire elemental spawning outside caves. Reference: https://trello.com/c/WfB4VaU8/4831-fire-elementals-usually-destroy-the-special-structure-that-they-live-at
                    }
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
		RuinarchListPool<LocationGridTile>.Release(locationChoices);
		RuinarchListPool<LocationStructure>.Release(allSpecialStructures);
		yield return null;
	}
	private IEnumerator CaveMonsterGeneration() {
		Region region = GridMap.Instance.allRegions.First();
		if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
			List<LocationStructure> caves = region.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
			List<LocationStructure> orderedStructures = RuinarchListPool<LocationStructure>.Claim();
			orderedStructures.AddRange(caves.OrderByDescending(x => x.tiles.Count));			
			// if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			// 	TutorialCaveMonsterGeneration(caves);
			// } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			// 	OonaCaveMonsterGeneration(caves);
			// } else 
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
				IcalawaCaveMonsterGeneration(orderedStructures);
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
					for (int j = 0; j < orderedStructures.Count; j++) {
						if (GameUtilities.RollChance(40)) {
							LocationStructure cave = orderedStructures[j];
							if (cave.passableTiles.Count == 0) { continue; }
							if (GameUtilities.RollChance(50)) {
								//spawn 2-4 ghosts
								int ghosts = Random.Range(2, 5);
								for (int k = 0; k < ghosts; k++) {
									CreateMonster(SUMMON_TYPE.Ghost, cave.passableTiles);
								}	
							} else {
								//spawn 2-4 to Skeletons
								List<CharacterClass> randomClassChoices = CharacterManager.Instance.GetNormalCombatantClasses();
								int skeletons = Random.Range(2, 5);
								for (int k = 0; k < skeletons; k++) {
									CreateMonster(SUMMON_TYPE.Skeleton, cave.passableTiles, className: CollectionUtilities.GetRandomElement(randomClassChoices).className, faction: FactionManager.Instance.undeadFaction);
								}	
							}
						}
						
					}
				} else {
					// WeightedDictionary<MonsterSetting> monsterChoices = caveData.monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
					List<LocationGridTile> locationChoices = RuinarchListPool<LocationGridTile>.Claim();
					for (int j = 0; j < orderedStructures.Count; j++) {
						LocationStructure structure = orderedStructures[j];
						Cave cave = structure as Cave;
						Assert.IsNotNull(cave);
						if (cave.residents.Count > 0 || cave.passableTiles.Count == 0) {
							//if cave already has occupants, then do not generate monsters for that cave
							continue;
						}
						if (cave.hasConnectedMine) {
							//do not spawn monsters on caves with currently connected mines.
							//Reference: https://trello.com/c/oFzZ2tV7/4811-monsters-should-no-longer-spawn-in-caves-connected-to-a-claimed-mine
							continue; 
						}
						if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom && CharacterManager.Instance.GenerateRatmen(cave, GameUtilities.RandomBetweenTwoNumbers(1, 3), 8)) {
							//Ratmen has bee generated
						} else {
							BiomeDivision biomeDivision = cave.region.biomeDivisionComponent.GetBiomeDivisionThatTileBelongsTo(cave.tiles.First());
							locationChoices.Clear();
							locationChoices.AddRange(cave.passableTiles);

							MonsterMigrationBiomeAtomizedData chosenMonster = biomeDivision.GetRandomMonsterFromFaunaList();
							int randomAmount = GameUtilities.RandomBetweenTwoNumbers(chosenMonster.minRange, chosenMonster.maxRange); ;
							for (int k = 0; k < randomAmount; k++) {
								Summon summon = CreateMonster(chosenMonster.monsterType, locationChoices, cave, faction: FactionManager.Instance.GetDefaultFactionForMonster(chosenMonster.monsterType));
								if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
									summon.traitContainer.AddTrait(summon, "Mighty");
								}
								locationChoices.Remove(summon.gridTileLocation);
								if (locationChoices.Count == 0) {
									Debug.LogWarning($"Ran out of grid tiles to place monsters at structure {cave.name}");
									break;
								}
							}
							if (locationChoices.Count == 0) {
								Debug.LogWarning($"Ran out of grid tiles to place monsters at structure {cave.name}");
								break;
							}
						}
					}
					RuinarchListPool<LocationGridTile>.Release(locationChoices);
				}
					
			}
			RuinarchListPool<LocationStructure>.Release(orderedStructures);
		}
		yield return null;
	}
	//private List<Area> GetHexTileCountOfCave(LocationStructure caveStructure) {
	//	List<Area> tiles = new List<Area>();
	//	for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++) {
	//		LocationGridTile tile = caveStructure.unoccupiedTiles.ElementAt(i);
	//		if (tiles.Contains(tile.area) == false) {
	//			tiles.Add(tile.area);
	//		}
	//	}
	//	return tiles;
	//}

	#region Icalawa
	private void IcalawaCaveMonsterGeneration(List<LocationStructure> caves) {
		List<LocationStructure> shuffledCaves = RuinarchListPool<LocationStructure>.Claim();
        for (int i = 0; i < caves.Count; i++) {
			LocationStructure s = caves[i];
            if (s.unoccupiedTiles.Count > 0) {
				shuffledCaves.Add(s);
			}
        }
		CollectionUtilities.Shuffle(shuffledCaves);
		if (shuffledCaves.Count > 0) {
			LocationStructure ratmenCave = shuffledCaves[0];
			shuffledCaves.Remove(ratmenCave);
			CharacterManager.Instance.GenerateRatmen(ratmenCave, 3, 100);
		}
		
		for (int j = 0; j < shuffledCaves.Count; j++) {
			LocationStructure cave = shuffledCaves[j];
			if (cave.residents.Count > 0) {
				//if cave already has occupants, then do not generate monsters for that cave
				continue;
			}
			//List<Area> hexTilesOfCave = GetHexTileCountOfCave(cave);
			if (j < 4) {
				CreateMonster(SUMMON_TYPE.Wurm, cave.unoccupiedTiles, cave);
			}
		}
	}
	#endregion
	
	#region Pangat Loo
	private void PangatLooLandmarkMonsterGeneration() {
		//skeletons at ancient graveyard
		List<LocationStructure> graveyards = LandmarkManager.Instance.GetStructuresOfType(STRUCTURE_TYPE.ANCIENT_GRAVEYARD);
		if (graveyards != null) {
			for (int i = 0; i < graveyards.Count; i++) {
				LocationStructure structure = graveyards[i];
				int randomAmount = 2;
				for (int k = 0; k < randomAmount; k++) {
					CreateMonster(SUMMON_TYPE.Skeleton, structure.settlementLocation, structure.region, structure, FactionManager.Instance.undeadFaction);
				}
			}
		}
	}
	#endregion
}
