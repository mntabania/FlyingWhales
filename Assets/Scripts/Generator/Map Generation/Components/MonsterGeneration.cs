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
		LocationGridTile targetTile = CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles);
		CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);
	}
    private Summon CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, string className = "", Faction faction = null, params Area[] territories) {
		var chosenTile = homeStructure != null ? CollectionUtilities.GetRandomElement(homeStructure.unoccupiedTiles) : CollectionUtilities.GetRandomElement(locationChoices);
		
		Assert.IsNotNull(chosenTile, $"Chosen tile for {summonType.ToString()} is null!");

		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.GetDefaultFactionForMonster(summonType), null, chosenTile.parentMap.region, className: className);
		CharacterManager.Instance.PlaceSummonInitially(summon, chosenTile);
		if (homeStructure != null) {
			summon.MigrateHomeStructureTo(homeStructure);	
		} else {
			summon.SetTerritory(chosenTile.area, false);
			if (territories != null) {
				for (int i = 0; i < territories.Length; i++) {
					Area territory = territories[i];
					summon.SetTerritory(territory, false);
				}
			}	
		}
		return summon;
	}
    private void CreateCharacter(RACE race, string className, GENDER gender, BaseSettlement settlementOnTile, LocationStructure structure, Faction faction = null) {
        Character character = CharacterManager.Instance.CreateNewCharacter(className, race, gender, faction ?? FactionManager.Instance.neutralFaction, settlementOnTile, settlementOnTile.region, structure);
        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(structure.passableTiles) ?? CollectionUtilities.GetRandomElement(structure.tiles);
        character.CreateMarker();
        character.InitialCharacterPlacement(targetTile);
    }
    #endregion

    private IEnumerator LandmarkMonsterGeneration() {
	    if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			PangatLooLandmarkMonsterGeneration();
		}
		List<LocationStructure> allSpecialStructures = LandmarkManager.Instance.GetAllSpecialStructures();
		List<LocationGridTile> locationChoices = new List<LocationGridTile>();
		for (int i = 0; i < allSpecialStructures.Count; i++) {
			LocationStructure structure = allSpecialStructures[i];
			if (structure.structureType != STRUCTURE_TYPE.CAVE) {
				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo && structure.structureType == STRUCTURE_TYPE.ANCIENT_GRAVEYARD) {
					continue; //do not spawn other monsters in ancient graveyard for Pangat Loo since there are already skeletons there.
				}
				Assert.IsNotNull(structure.occupiedArea, $"Occupied area of {structure.name} is null!");
				BiomeDivision biomeDivision = structure.region.biomeDivisionComponent.GetBiomeDivisionThatTileBelongsTo(structure.tiles.First());
				if (structure is RuinedZoo) {
					continue; //skip
				}
				if (GameUtilities.RollChance(70)) {
					locationChoices.Clear();
					locationChoices.AddRange(structure.passableTiles);
					
                    MonsterMigrationBiomeAtomizedData chosenMMonster = biomeDivision.GetRandomMonsterFromFaunaList();
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
		RuinarchListPool<LocationStructure>.Release(allSpecialStructures);
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
							if (cave.unoccupiedTiles.Count == 0) { continue; }
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
						if (cave.residents.Count > 0 || cave.passableTiles.Count == 0) {
							//if cave already has occupants, then do not generate monsters for that cave
							continue;
						}
						BiomeDivision biomeDivision = cave.region.biomeDivisionComponent.GetBiomeDivisionThatTileBelongsTo(cave.tiles.First());
						if (GameUtilities.RollChance(70)) {
							locationChoices.Clear();
							locationChoices.AddRange(cave.passableTiles);
					
							MonsterMigrationBiomeAtomizedData chosenMMonster = biomeDivision.GetRandomMonsterFromFaunaList();
							int randomAmount = GameUtilities.RandomBetweenTwoNumbers(chosenMMonster.minRange, chosenMMonster.maxRange);;
							for (int k = 0; k < randomAmount; k++) {
								Summon summon = CreateMonster(chosenMMonster.monsterType, locationChoices, cave, faction: FactionManager.Instance.GetDefaultFactionForMonster(chosenMMonster.monsterType));
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
	private List<Area> GetHexTileCountOfCave(LocationStructure caveStructure) {
		List<Area> tiles = new List<Area>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++) {
			LocationGridTile tile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (tiles.Contains(tile.area) == false) {
				tiles.Add(tile.area);
			}
		}
		return tiles;
	}

	#region Icalawa
	private void IcalawaCaveMonsterGeneration(List<LocationStructure> caves) {
		List<LocationStructure> shuffledCaves = CollectionUtilities.Shuffle(caves.Where(c => c.unoccupiedTiles.Count > 0).ToList());
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
			List<Area> hexTilesOfCave = GetHexTileCountOfCave(cave);
			if (j < 4) {
				CreateMonster(SUMMON_TYPE.Wurm, cave.unoccupiedTiles.ToList(), cave, territories: hexTilesOfCave.ToArray());
			}
		}
	}
	#endregion
	
	#region Pangat Loo
	private void PangatLooLandmarkMonsterGeneration() {
		//skeletons at ancient graveyard
		List<LocationStructure> graveyards = LandmarkManager.Instance.GetSpecialStructuresOfType(STRUCTURE_TYPE.ANCIENT_GRAVEYARD);
		for (int i = 0; i < graveyards.Count; i++) {
			LocationStructure structure = graveyards[i];
			int randomAmount = 2;
			for (int k = 0; k < randomAmount; k++) {
				CreateMonster(SUMMON_TYPE.Skeleton, structure.settlementLocation, structure.region, structure, FactionManager.Instance.undeadFaction);
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
}
