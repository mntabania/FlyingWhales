﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Tile_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class SettlementGeneration : MapGenerationComponent {

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating settlements...");
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasTileWithFeature(TileFeatureDB.Inhabited_Feature)) {
				yield return MapGenerator.Instance.StartCoroutine(CreateSettlement(region, data));
			}
			// region.innerMap.PlaceBuildSpotTileObjects();
		}
		ApplyPreGeneratedRelationships(data);
		yield return null;
	}
	private IEnumerator CreateSettlement(Region region, MapGenerationData data) {
		List<HexTile> settlementTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		if (WorldConfigManager.Instance.isTutorialWorld) {
			Assert.IsTrue(settlementTiles.Count == 4, "Settlement tiles of demo build is not 4!");
		}
		// //create village landmark on settlement tiles
		// for (int i = 0; i < settlementTiles.Count; i++) {
		// 	HexTile villageTile = settlementTiles[i];
		// 	LandmarkManager.Instance.CreateNewLandmarkOnTile(villageTile, LANDMARK_TYPE.VILLAGE);
		// }

		List<HexTileIsland> settlementIslands = GetSettlementIslandsInRegion(region);

		for (int i = 0; i < settlementIslands.Count; i++) {
			HexTileIsland island = settlementIslands[i];
			yield return MapGenerator.Instance.StartCoroutine(GenerateRandomSettlement(region, data, island.tilesInIsland));
		}
	}
	private IEnumerator GenerateRandomSettlement(Region region, MapGenerationData data, List<HexTile> settlementTiles) {
		List<RACE> validRaces = WorldSettings.Instance.worldSettingsData.races;
		RACE neededRace = GetFactionRaceForRegion(region);
		if (validRaces.Contains(neededRace)) {
			Faction faction = GetFactionToOccupySettlement(neededRace);
			LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);

			NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, settlementTiles.First());
			// npcSettlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
			LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
			if (faction.race == RACE.HUMANS) {
				npcSettlement.SetSettlementType(SETTLEMENT_TYPE.Default_Human);
			} else if (faction.race == RACE.ELVES) {
				npcSettlement.SetSettlementType(SETTLEMENT_TYPE.Default_Elf);
			}
			List<StructureSetting> structureSettings;
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
				structureSettings = new List<StructureSetting>() {
					new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE)
				};
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
				structureSettings = new List<StructureSetting>() {
					new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
				};
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
				structureSettings = new List<StructureSetting>() {
					new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE),
				};
			} else {
				structureSettings = GenerateFacilities(npcSettlement, faction, Random.Range(2, 4));
				int generatedFacilities = structureSettings.Count;
				for (int i = 0; i < generatedFacilities; i++) {
					StructureSetting structureSetting = structureSettings[i];
					if (structureSetting.structureType.IsFacilityStructure()) {
						var dwellingCount = structureSetting.structureType == STRUCTURE_TYPE.CITY_CENTER ? 2 : Random.Range(1, 3);
						//add 1 or 2 dwellings per facility
						for (int j = 0; j < dwellingCount; j++) {
							structureSettings.Add(new StructureSetting(STRUCTURE_TYPE.DWELLING, faction.factionType.mainResource));	
						}
						
					}
				}
				// structureSettings = new List<StructureSetting>() {
				// 	new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE),
				// 	new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				// };
			}

			Assert.IsTrue(structureSettings.First().structureType == STRUCTURE_TYPE.CITY_CENTER);
			yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
			// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltStructuresForSettlement(npcSettlement, region.innerMap, structureSettings.ToArray()));
			yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjects());

			if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				//Add combatant classes from faction type to location class manager
				for (int i = 0; i < faction.factionType.combatantClasses.Count; i++) {
					npcSettlement.classManager.AddCombatantClass(faction.factionType.combatantClasses[i]);
				}
				List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data);
			
				List<TileObject> objectsInDwellings =
					npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
				for (int i = 0; i < objectsInDwellings.Count; i++) {
					TileObject tileObject = objectsInDwellings[i];
					tileObject.UpdateOwners();
				}

				CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);	
			}
			npcSettlement.Initialize();
		}
	}
	private IEnumerator EnsuredStructurePlacement(Region region, List<StructureSetting> structureSettings, NPCSettlement npcSettlement) {
		List<StructureSetting> unplacedStructures = new List<StructureSetting>();
		List<StructureSetting> structuresToPlace = new List<StructureSetting>(structureSettings);
		for (int i = 0; i < 2; i++) {
			yield return MapGenerator.Instance.StartCoroutine(PlaceStructures(region, structuresToPlace, npcSettlement));
			//check whole structure list to verify if all needed structures were placed.
			unplacedStructures.Clear();
			unplacedStructures.AddRange(structureSettings);
			for (int j = 0; j < npcSettlement.allStructures.Count; j++) {
				LocationStructure structure = npcSettlement.allStructures[j];
				if (structure is ManMadeStructure manMadeStructure) {
					for (int k = 0; k < unplacedStructures.Count; k++) {
						StructureSetting structureSetting = unplacedStructures[k];
						if (manMadeStructure.structureType == structureSetting.structureType) {
							//&& manMadeStructure.wallsAreMadeOf == structureSetting.resource
							unplacedStructures.RemoveAt(k);
							break;
						}
					}
				}
			}
			if (unplacedStructures.Count == 0) {
				break; //no more unplaced structures
			}
			else {
				//make structure setting list and unplaced structures list identical so that unplaced structures will tried to be placed on next iteration.
				structuresToPlace.Clear();
				structuresToPlace.AddRange(unplacedStructures);
				if (i + 1 == 2) {
					//last iteration
					string summary = $"Was unable to place the following structures:";
					for (int j = 0; j < unplacedStructures.Count; j++) {
						summary = $"{summary}\n- {unplacedStructures[j].ToString()}";
					}
					Debug.Log(summary);
				}
			}
		}
	}
	private IEnumerator PlaceStructures(Region region, List<StructureSetting> structureSettings, NPCSettlement npcSettlement) {
		for (int i = 0; i < structureSettings.Count; i++) {
			StructureSetting structureSetting = structureSettings[i];
			if (structureSetting.structureType == STRUCTURE_TYPE.CITY_CENTER) {
				yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, structureSetting));
			} else {
				List<StructureConnector> availableStructureConnectors = npcSettlement.GetAvailableStructureConnectors();
				availableStructureConnectors = CollectionUtilities.Shuffle(availableStructureConnectors);
				List<GameObject> prefabChoices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
				prefabChoices = CollectionUtilities.Shuffle(prefabChoices);
				for (int j = 0; j < prefabChoices.Count; j++) {
					GameObject prefabGO = prefabChoices[j];
					LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
					StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, region.innerMap, out var connectorIndex, out LocationGridTile tileToPlaceStructure);
					if (validConnector != null) {
						//instantiate structure object at tile.
						LocationStructure createdStructure = LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, prefabGO, tileToPlaceStructure);
						// validConnector.SetOpenState(false);
						// if (createdStructure is ManMadeStructure manMadeStructure) {
						// 	StructureConnector chosenConnector = manMadeStructure.structureObj.connectors[connectorIndex];
						// 	chosenConnector.SetOpenState(false);
						// }
						break; //stop loop since structure was already placed.
					}
				}
			}
		}
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		if (scenarioMapData.villageSettlementTemplates != null) {
			for (int i = 0; i < scenarioMapData.villageSettlementTemplates.Length; i++) {
				SettlementTemplate settlementTemplate = scenarioMapData.villageSettlementTemplates[i];
				HexTile[] tilesInSettlement = settlementTemplate.GetTilesInTemplate(GridMap.Instance.map);

				Region region = tilesInSettlement[0].region;
				
				// //create village landmark on settlement tiles
				// for (int j = 0; j < tilesInSettlement.Length; j++) {
				// 	HexTile villageTile = tilesInSettlement[j];
				// 	LandmarkManager.Instance.CreateNewLandmarkOnTile(villageTile, LANDMARK_TYPE.VILLAGE);
				// }
				
				//create faction
				Faction faction = GetFactionForScenario(settlementTemplate);

				LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
			
				NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, tilesInSettlement.First());
				npcSettlement.SetSettlementType(settlementTemplate.settlementType);
				// npcSettlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
				LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
				
				StructureSetting[] structureSettings = settlementTemplate.structureSettings;
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings.ToList(), npcSettlement));
				// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltStructuresForSettlement(npcSettlement, region.innerMap, structureSettings));
				yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjects());
				
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				List<Character> spawnedCharacters = CreateSettlementResidentsForScenario(dwellingCount, npcSettlement, faction, data, settlementTemplate.minimumVillagerCount);

				//update objects owners in dwellings
				List<TileObject> objectsInDwellings = npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
				for (int j = 0; j < objectsInDwellings.Count; j++) {
					TileObject tileObject = objectsInDwellings[j];
					tileObject.UpdateOwners();
				}
			
				CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);
				npcSettlement.Initialize();
				yield return null;
			}
			ApplyPreGeneratedRelationships(data);
		}
	}
	private Faction GetFactionForScenario(SettlementTemplate settlementTemplate) {
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			if (settlementTemplate.factionRace == RACE.ELVES) {
				List<Faction> factions = FactionManager.Instance.GetMajorFactionWithRace(settlementTemplate.factionRace);
				if (factions != null) {
					return CollectionUtilities.GetRandomElement(factions);
				} else {
					return CreateDefaultFaction(settlementTemplate.factionRace); 
				}
			} else {
				return CreateDefaultFaction(settlementTemplate.factionRace);
			}
		} else {
			Faction faction = CreateDefaultFaction(settlementTemplate.factionRace);
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
				faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive);
			}
			return faction;
		}
	}
	private static Faction CreateDefaultFaction(RACE race) {
		FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(race);
		Faction faction = FactionManager.Instance.CreateNewFaction(factionType);
		faction.factionType.SetAsDefault();
		return faction;
	}
	private List<Character> CreateSettlementResidentsForScenario(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data, int providedCitizenCount = -1) {
		//Add combatant classes from faction type to location class manager
		for (int j = 0; j < faction.factionType.combatantClasses.Count; j++) {
			npcSettlement.classManager.AddCombatantClass(faction.factionType.combatantClasses[j]);
		}

		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			List<string> elfClassesInOrder = new List<string> {"Craftsman", "Craftsman", "Peasant", "Peasant", "Miner", "Miner"};
			List<string> humanClassesPriority = new List<string>() { "Knight", "Shaman", "Knight", "Shaman", "Knight",  "Shaman" };
			List<Character> createdCharacters = new List<Character>();
			int citizenCount = 0;
			for (int i = 0; i < dwellingCount; i++) {
				List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
				if (availableDwellings.Count == 0) {
					break; //no more dwellings
				}
				Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
				if (i >= 0 && i < 9) {
					int coupleChance = 35;
					int afterCoupleGenerationAmount = citizenCount + 2;
					if (afterCoupleGenerationAmount <= 18) {
						coupleChance = 100;
					}

					if (GameUtilities.RollChance(coupleChance)) {
						//spawn human couples
						//couple
						List<Couple> couples = GetAvailableCouplesToBeSpawned(faction.race, data);
						if (couples.Count > 0) {
							Couple couple = CollectionUtilities.GetRandomElement(couples);
							createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement, 
								humanClassesPriority.Count > 0 ? humanClassesPriority.First() : string.Empty,
								humanClassesPriority.Count > 1 ? humanClassesPriority[1] : string.Empty));
							if (humanClassesPriority.Count > 1) {
								humanClassesPriority.RemoveRange(0, 2);	
							} else if (humanClassesPriority.Count > 0) {
								humanClassesPriority.RemoveAt(0);	
							}
							citizenCount += 2;
						} else {
							//no more couples left	
							List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
							if (siblingCouples.Count > 0) {
								Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
								createdCharacters.AddRange( SpawnCouple(couple, dwelling, faction, npcSettlement, 
									humanClassesPriority.Count > 0 ? humanClassesPriority.First() : string.Empty,
									humanClassesPriority.Count > 1 ? humanClassesPriority[1] : string.Empty));
								if (humanClassesPriority.Count > 1) {
									humanClassesPriority.RemoveRange(0, 2);	
								} else if (humanClassesPriority.Count > 0) {
									humanClassesPriority.RemoveAt(0);	
								}
								citizenCount += 2;
							} else {
								//no more sibling Couples	
								//spawn single
								TrySpawnSingleCharacter(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount, humanClassesPriority.Count > 0 ? humanClassesPriority.First() : string.Empty);
								if (humanClassesPriority.Count > 0) {
									humanClassesPriority.RemoveAt(0);	
								}
							}
						}
					} else {
						//spawn single
						TrySpawnSingleCharacter(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount, humanClassesPriority.Count > 0 ? humanClassesPriority.First() : string.Empty);
						if (humanClassesPriority.Count > 0) {
							humanClassesPriority.RemoveAt(0);	
						}
					}
				} else {
					if (citizenCount >= 24) {
						break;
					}
					//spawn peasant elves couple
					if (elfClassesInOrder.Count == 0) {
						elfClassesInOrder.Add("Craftsman");
						elfClassesInOrder.Add("Craftsman");
						elfClassesInOrder.Add("Peasant");
						elfClassesInOrder.Add("Peasant");
						elfClassesInOrder.Add("Miner");
						elfClassesInOrder.Add("Miner");
					}
					List<Couple> couples = GetAvailableCouplesToBeSpawned(RACE.ELVES, data);
					if (couples.Count > 0) {
						Couple couple = CollectionUtilities.GetRandomElement(couples);
						createdCharacters.Add(SpawnCharacter(couple.character1, elfClassesInOrder.First(), dwelling, faction, npcSettlement));
						createdCharacters.Add(SpawnCharacter(couple.character2, elfClassesInOrder[1], dwelling, faction, npcSettlement));
						elfClassesInOrder.RemoveRange(0, 2);
						citizenCount += 2;
					} else {
						//no more couples left	
						List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(RACE.ELVES, data);
						if (siblingCouples.Count > 0) {
							Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
							createdCharacters.Add(SpawnCharacter(couple.character1, elfClassesInOrder.First(), dwelling, faction, npcSettlement));
							createdCharacters.Add(SpawnCharacter(couple.character2, elfClassesInOrder[1], dwelling, faction, npcSettlement));
							elfClassesInOrder.RemoveRange(0, 2);
							citizenCount += 2;
						} else {
							//no more sibling Couples	
							//spawn single
							TrySpawnSingleCharacter(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount, elfClassesInOrder.First());
							elfClassesInOrder.RemoveAt(0);
						}
					}
				}
			}
			return createdCharacters;
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			//always spawn couples for Icalawa since there are always 6 dwellings and citizen count needs to be always 12
			return GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data, providedCitizenCount, coupleChanceOverride: 100);
		} else {
			return GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data, providedCitizenCount);	
		}
	}
	private void TrySpawnSingleCharacter(NPCSettlement npcSettlement, Faction faction, MapGenerationData data, Dwelling dwelling, ref List<Character> createdCharacters, ref int citizenCount, string className = "") {
		PreCharacterData singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		string classString = string.IsNullOrEmpty(className) ? npcSettlement.classManager.GetCurrentClassToCreate() : className;
		if (singleCharacter != null) {
			createdCharacters.Add(SpawnCharacter(singleCharacter, classString, dwelling, faction, npcSettlement));
			citizenCount += 1;
		} else {
			//no more characters to spawn
			Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
			FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
			DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
			singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
			Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
			createdCharacters.Add(SpawnCharacter(singleCharacter, classString, dwelling, faction, npcSettlement));
			citizenCount += 1;
		}
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion

	#region Settlement Structures
	private List<StructureSetting> GenerateFacilities(NPCSettlement settlement, Faction faction, int facilityCount) {
		List<StructureSetting> structures = new List<StructureSetting> { faction.factionType.GetStructureSettingFor(STRUCTURE_TYPE.CITY_CENTER) };
		List<STRUCTURE_TYPE> createdStructureTypes = new List<STRUCTURE_TYPE>();
		for (int i = 0; i < facilityCount; i++) {
			WeightedDictionary<StructureSetting> structuresChoices = GetStructureWeights(createdStructureTypes, faction);
			StructureSetting chosenSetting = structuresChoices.PickRandomElementGivenWeights();
			structures.Add(chosenSetting);
			createdStructureTypes.Add(chosenSetting.structureType);
		}
		return structures;
	}
	private WeightedDictionary<StructureSetting> GetStructureWeights(List<STRUCTURE_TYPE> structureTypes, Faction faction) {
		WeightedDictionary<StructureSetting> structureWeights = new WeightedDictionary<StructureSetting>();
		if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
			if (structureTypes.Contains(STRUCTURE_TYPE.APOTHECARY) == false) {
				//Apothecary: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.APOTHECARY, RESOURCE.WOOD), 6);
			}
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 1); //Farm: +1
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), structureTypes.Contains(STRUCTURE_TYPE.FARM) == false ? 15 : 2);
			// if (tile.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), structureTypes.Contains(STRUCTURE_TYPE.FARM) == false ? 15 : 2);
			// }
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD), structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD) == false ? 15 : 2);
			// if (tile.HasNeighbourWithFeature(TileFeatureDB.Wood_Source_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD), structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD) == false ? 15 : 2);
			// }
			if (structureTypes.Contains(STRUCTURE_TYPE.CEMETERY) == false) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 2);
			}
		} else if (faction.factionType.type == FACTION_TYPE.Human_Empire) {
			if (structureTypes.Contains(STRUCTURE_TYPE.MAGE_QUARTERS) == false) {
				//Mage Quarter: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE), 6);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.PRISON) == false) {
				//Prison: +3 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 3); //3
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.BARRACKS) == false) {
				//Barracks: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE));
			}
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 1); //Farm: +1
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), structureTypes.Contains(STRUCTURE_TYPE.FARM) == false ? 15 : 2);
			// if (tile.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), structureTypes.Contains(STRUCTURE_TYPE.FARM) == false ? 15 : 2);
			// }
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE), structureTypes.Contains(STRUCTURE_TYPE.MINE_SHACK) == false ? 15 : 2);
			// if (tile.HasNeighbourWithFeature(TileFeatureDB.Metal_Source_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE), structureTypes.Contains(STRUCTURE_TYPE.MINE_SHACK) == false ? 15 : 2);
			// }
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE), structureTypes.Contains(STRUCTURE_TYPE.HUNTER_LODGE) == false ? 15 : 2);
			// if (tile.HasNeighbourWithFeature(TileFeatureDB.Game_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE), structureTypes.Contains(STRUCTURE_TYPE.HUNTER_LODGE) == false ? 15 : 2);
			// }
			if (structureTypes.Contains(STRUCTURE_TYPE.CEMETERY) == false) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 2);
			}
		}
		return structureWeights;
	}
	#endregion

	#region Residents
	private List<Character> GenerateSettlementResidents(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data, int providedCitizenCount = -1, int coupleChanceOverride = -1) {
		List<Character> createdCharacters = new List<Character>();
		int citizenCount = 0;
		for (int i = 0; i < dwellingCount; i++) {
			int roll = Random.Range(0, 100);
			int coupleChance;
			if (coupleChanceOverride != -1) {
				coupleChance = coupleChanceOverride;
			} else {
				coupleChance = 35; //35
				if (providedCitizenCount > 0) {
					if (citizenCount >= providedCitizenCount) {
						break;
					}
				
					//if number of citizens are provided, check if the current citizen count + 2 (Couple), is still less than the given amount
					//if it is, then increase chance to spawn a couple
					int afterCoupleGenerationAmount = citizenCount + 2;
					if (afterCoupleGenerationAmount < providedCitizenCount) {
						coupleChance = 70;
					}
				}	
			}

			List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellings.Count == 0) {
				break; //no more dwellings
			}

			Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
			if (roll < coupleChance) {
				//couple
				List<Couple> couples = GetAvailableCouplesToBeSpawned(faction.race, data);
				if (couples.Count > 0) {
					Couple couple = CollectionUtilities.GetRandomElement(couples);
					createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement));
					citizenCount += 2;
				} else {
					//no more couples left	
					List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
					if (siblingCouples.Count > 0) {
						Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
						createdCharacters.AddRange( SpawnCouple(couple, dwelling, faction, npcSettlement));
						citizenCount += 2;
					} else {
						//no more sibling Couples	
						PreCharacterData singleCharacter =
							GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
						if (singleCharacter != null) {
							createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, npcSettlement));
							citizenCount += 1;
						} else {
							//no more characters to spawn
							Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
							FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
							DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
							singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
							Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
							createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, npcSettlement));
							citizenCount += 1;
						}
					}
				}
			} else {
				//single
				PreCharacterData singleCharacter =
					GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
				if (singleCharacter != null) {
					createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, npcSettlement));
					citizenCount += 1;
				} else {
					//no more characters to spawn
					Debug.LogWarning("Could not find any more characters to spawn");
					FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
					DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
					singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
					Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
					createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, npcSettlement));
					citizenCount += 1;
				}
			}
		}
		return createdCharacters;
	}
	private List<Couple> GetAvailableCouplesToBeSpawned(RACE race, MapGenerationData data) {
		List<Couple> couples = new List<Couple>();
		List<FamilyTree> familyTrees = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, DatabaseManager.Instance.familyTreeDatabase);
					if (lover != null && lover.hasBeenSpawned == false) {
						Couple couple = new Couple(familyMember, lover);
						if (couples.Contains(couple) == false) {
							couples.Add(couple);
						}
					}
				}
			}
		}
		return couples;
	}
	private List<Couple> GetAvailableSiblingCouplesToBeSpawned(RACE race, MapGenerationData data) {
		List<Couple> couples = new List<Couple>();
		List<FamilyTree> familyTrees = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			if (familyTree.children != null && familyTree.children.Count >= 2) {
				List<PreCharacterData> unspawnedChildren = familyTree.children.Where(x => x.hasBeenSpawned == false).ToList();
				if (unspawnedChildren.Count >= 2) {
					PreCharacterData random1 = CollectionUtilities.GetRandomElement(unspawnedChildren);
					unspawnedChildren.Remove(random1);
					PreCharacterData random2 = CollectionUtilities.GetRandomElement(unspawnedChildren);
					Couple couple = new Couple(random1, random2);
					if (couples.Contains(couple) == false) {
						couples.Add(couple);
					}
				}
			}
		}
		return couples;
	}
	private PreCharacterData GetAvailableSingleCharacterForSettlement(RACE race, MapGenerationData data, NPCSettlement npcSettlement) {
		List<PreCharacterData> availableCharacters = new List<PreCharacterData>();
		List<FamilyTree> familyTrees = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, DatabaseManager.Instance.familyTreeDatabase);
					//check if the character has a lover, if it does, check if its lover has been spawned, if it has, check that the lover was spawned in a different npcSettlement
					if (lover == null || lover.hasBeenSpawned == false || 
					    CharacterManager.Instance.GetCharacterByID(lover.id).homeSettlement != npcSettlement) {
						availableCharacters.Add(familyMember);
					}
				}
			}
		}

		if (availableCharacters.Count > 0) {
			return CollectionUtilities.GetRandomElement(availableCharacters);
		}
		return null;
	}
	private List<Dwelling> GetAvailableDwellingsAtSettlement(NPCSettlement npcSettlement) {
		List<Dwelling> dwellings = new List<Dwelling>();
		if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
			List<LocationStructure> locationStructures = npcSettlement.structures[STRUCTURE_TYPE.DWELLING];
			for (int i = 0; i < locationStructures.Count; i++) {
				LocationStructure currStructure = locationStructures[i];
				Dwelling dwelling = currStructure as Dwelling;
				if (dwelling.residents.Count == 0) {
					dwellings.Add(dwelling);	
				}
			}
		}
		return dwellings;
	}
	private List<Character> SpawnCouple(Couple couple, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement, string className1 = "", string className2 = "") {
		List<Character> characters = new List<Character>() {
			SpawnCharacter(couple.character1, string.IsNullOrEmpty(className1) ? npcSettlement.classManager.GetCurrentClassToCreate() : className1, dwelling, faction, npcSettlement),
			SpawnCharacter(couple.character2, string.IsNullOrEmpty(className2) ? npcSettlement.classManager.GetCurrentClassToCreate() : className2, dwelling, faction, npcSettlement)	
		};
		return characters;
	}
	private Character SpawnCharacter(PreCharacterData data, string className, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement) {
		return CharacterManager.Instance.CreateNewCharacter(data, className, faction, npcSettlement, dwelling);
	}
	#endregion

	#region Relationships
	private void ApplyPreGeneratedRelationships(MapGenerationData data) {
		foreach (var pair in DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary) {
			for (int i = 0; i < pair.Value.Count; i++) {
				FamilyTree familyTree = pair.Value[i];
				for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
					PreCharacterData characterData = familyTree.allFamilyMembers[j];
					if (characterData.hasBeenSpawned) {
						Character character = CharacterManager.Instance.GetCharacterByID(characterData.id); 
						RelationshipManager.Instance.ApplyPreGeneratedRelationships(data, characterData, character);
					}
				}
			}
		}
	}
	#endregion

	#region Settlement Generation Utilities
	private RACE GetFactionRaceForRegion(Region region) {
		if (region.coreTile.biomeType == BIOMES.FOREST || region.coreTile.biomeType == BIOMES.SNOW) {
			return RACE.ELVES;
		} else if (region.coreTile.biomeType == BIOMES.DESERT || region.coreTile.biomeType == BIOMES.GRASSLAND) {
			return RACE.HUMANS;
		}
		throw new Exception($"Could not get race type for region with biome type {region.coreTile.biomeType.ToString()}");
	}
	private LOCATION_TYPE GetLocationTypeForRace(RACE race) {
		switch (race) {
			case RACE.HUMANS:
			case RACE.ELVES:
				return LOCATION_TYPE.SETTLEMENT;
			default:
				throw new Exception($"There was no location type provided for race {race.ToString()}");
		}
	}
	private Faction GetFactionToOccupySettlement(RACE race) {
		FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(race);
		List<Faction> factions = FactionManager.Instance.GetMajorFactionWithRace(race);
		Faction chosenFaction;
		if (factions == null) {
			chosenFaction = FactionManager.Instance.CreateNewFaction(factionType);
			chosenFaction.factionType.SetAsDefault();
		} else {
			if (GameUtilities.RollChance(35)) {
				chosenFaction = CollectionUtilities.GetRandomElement(factions);
			} else {
				chosenFaction = FactionManager.Instance.CreateNewFaction(factionType);
				chosenFaction.factionType.SetAsDefault();
			}
		}
		return chosenFaction;
	}
	private List<HexTileIsland> GetSettlementIslandsInRegion(Region region) {
		List<HexTile> inhabitedTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		
		List<HexTileIsland> islands = new List<HexTileIsland>();
		for (int i = 0; i < inhabitedTiles.Count; i++) {
			HexTile tile = inhabitedTiles[i];
			HexTileIsland island = new HexTileIsland(tile);
			islands.Add(island);
		}

		for (int i = 0; i < islands.Count; i++) {
			HexTileIsland island = islands[i];
			for (int j = 0; j < islands.Count; j++) {
				HexTileIsland otherIsland = islands[j];
				if (island != otherIsland) {
					if (island.IsConnectedToThisIsland(otherIsland)) {
						island.MergeIsland(otherIsland);
					}
				}
			}
		}

		return islands.Where(x => x.tilesInIsland.Count > 0).ToList();
	}
	#endregion
	
}

public class HexTileIsland {
	public List<HexTile> tilesInIsland { get; }

	public HexTileIsland(HexTile tile) {
		tilesInIsland = new List<HexTile> {tile};
	}
	public void MergeIsland(HexTileIsland otherIsland) {
		tilesInIsland.AddRange(otherIsland.tilesInIsland);
		otherIsland.ClearIsland();
	}
	public void ClearIsland() {
		tilesInIsland.Clear();
	}
	public bool IsConnectedToThisIsland(HexTileIsland otherIsland) {
		for (int i = 0; i < tilesInIsland.Count; i++) {
			HexTile tileInIsland = tilesInIsland[i];
			for (int j = 0; j < tileInIsland.AllNeighbours.Count; j++) {
				HexTile neighbour = tileInIsland.AllNeighbours[j];
				if (otherIsland.tilesInIsland.Contains(neighbour)) {
					return true;
				}	
			}
		}
		return false;
	}
}

public class Couple : IEquatable<Couple> {
	public PreCharacterData character1 { get; }
	public PreCharacterData character2 { get; }

	public Couple(PreCharacterData _character1, PreCharacterData _character2) {
		character1 = _character1;
		character2 = _character2;
	}
	public bool Equals(Couple other) {
		if (other == null) {
			return false;
		}
		return (character1.id == other.character1.id && character2.id == other.character2.id) ||
		       (character1.id == other.character2.id && character2.id == other.character1.id);
	}
	public override bool Equals(object obj) {
		return Equals(obj as  Couple);
	}
	public override int GetHashCode() {
		return character1.id + character2.id;
	}
}