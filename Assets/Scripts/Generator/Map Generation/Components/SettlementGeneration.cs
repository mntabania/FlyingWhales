using System;
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
			Assert.IsTrue(region.HasTileWithFeature(TileFeatureDB.Inhabited_Feature));
			yield return MapGenerator.Instance.StartCoroutine(CreateSettlements(region, data));
			
		}
		ApplyPreGeneratedCharacterRelationships(data);
		for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++) {
			NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
			settlement.migrationComponent.ForceRandomizePerHourIncrement();
		}
		yield return null;
	}
	private IEnumerator CreateSettlements(Region region, MapGenerationData data) {
		foreach (var setting in data.determinedVillages) {
			FactionTemplate factionTemplate = setting.Key;
			Faction faction = FactionManager.Instance.CreateNewFaction(factionTemplate.factionType, factionTemplate.name, factionTemplate.factionEmblem);
			faction.factionType.SetAsDefault();
			LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
			for (int i = 0; i < setting.Value.Count; i++) {
				HexTile settlementTile = setting.Value[i];
				VillageSetting villageSetting = factionTemplate.villageSettings[i];
				NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, settlementTile);
				npcSettlement.SetName(villageSetting.villageName);
				LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
				SETTLEMENT_TYPE settlementType = LandmarkManager.Instance.GetSettlementTypeForRace(faction.race);
				npcSettlement.SetSettlementType(settlementType);
				
				var structureSettings = GenerateCityCenterAndDwellings(npcSettlement, faction, villageSetting);
				
				Assert.IsTrue(structureSettings.First().structureType == STRUCTURE_TYPE.CITY_CENTER);
				Assert.IsTrue(npcSettlement.tiles.Count > 0);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
				structureSettings = GenerateFacilities(npcSettlement, faction, villageSetting.GetRandomFacilityCount());
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
				yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsCoroutine());

				if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
					int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
					List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data);
					List<TileObject> objectsInDwellings = npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
					for (int j = 0; j < objectsInDwellings.Count; j++) {
						TileObject tileObject = objectsInDwellings[j];
						tileObject.UpdateOwners();
					}
					CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);	
				}
			}
		}
		
		// List<HexTile> settlementTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		// if (WorldConfigManager.Instance.isTutorialWorld) {
		// 	Assert.IsTrue(settlementTiles.Count == 4, "Settlement tiles of demo build is not 4!");
		// }
		// List<HexTileIsland> settlementIslands = GetSettlementIslandsInRegion(region);
		// for (int i = 0; i < settlementIslands.Count; i++) {
		// 	HexTileIsland island = settlementIslands[i];
		// 	yield return MapGenerator.Instance.StartCoroutine(GenerateRandomSettlement(region, data, island.tilesInIsland));
		// }
	}
	// private IEnumerator GenerateRandomSettlement(Region region, MapGenerationData data, List<HexTile> settlementTiles) {
	// 	List<RACE> validRaces = new List<RACE>() {RACE.ELVES, RACE.HUMANS};//WorldSettings.Instance.worldSettingsData.races;
	// 	RACE neededRace = GetFactionRaceForRegion(region);
	// 	if (validRaces.Contains(neededRace)) {
	// 		Faction faction = GetFactionToOccupySettlement(neededRace);
	// 		LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
	//
	// 		NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, settlementTiles.First());
	// 		LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
	// 		SETTLEMENT_TYPE settlementType = LandmarkManager.Instance.GetSettlementTypeForRace(faction.race);
	// 		npcSettlement.SetSettlementType(settlementType);
	// 		
	// 		var structureSettings = GenerateStructureSettings(npcSettlement, faction);
	//
	// 		Assert.IsTrue(structureSettings.First().structureType == STRUCTURE_TYPE.CITY_CENTER);
	// 		Assert.IsTrue(npcSettlement.tiles.Count > 0);
	// 		yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
	// 		yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsCoroutine());
	//
	// 		if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
	// 			int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
	// 			List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data);
	// 			List<TileObject> objectsInDwellings = npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
	// 			for (int i = 0; i < objectsInDwellings.Count; i++) {
	// 				TileObject tileObject = objectsInDwellings[i];
	// 				tileObject.UpdateOwners();
	// 			}
	// 			CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);	
	// 		}
	// 	}
	// }
	private List<StructureSetting> GenerateCityCenterAndDwellings(NPCSettlement p_npcSettlement, Faction p_faction, VillageSetting p_villageSetting) {
		List<StructureSetting> structureSettings;
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			structureSettings = new List<StructureSetting>() {
				new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE),
			};
		}
		else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			structureSettings = new List<StructureSetting>() {
				new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
			};
			for (int i = 0; i < 9; i++) {
				structureSettings.Add(new StructureSetting(STRUCTURE_TYPE.DWELLING, p_faction.factionType.mainResource));
			}
		}
		else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			structureSettings = new List<StructureSetting>() {
				new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
				new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE),
			};
		}
		else {
			// structureSettings = GenerateFacilities(p_npcSettlement, p_faction, p_villageSetting.GetRandomFacilityCount());
			structureSettings =  new List<StructureSetting> { new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, p_faction.factionType.mainResource) };
			int randomDwellings = p_villageSetting.GetRandomDwellingCount();
			for (int i = 0; i < randomDwellings; i++) {
				structureSettings.Add(new StructureSetting(STRUCTURE_TYPE.DWELLING, p_faction.factionType.mainResource));
			}
		}
		return structureSettings;
	}
	private IEnumerator EnsuredStructurePlacement(Region region, List<StructureSetting> structureSettings, NPCSettlement npcSettlement) {
		List<StructureSetting> unplacedStructures = new List<StructureSetting>();
		List<StructureSetting> structuresToPlace = new List<StructureSetting>(structureSettings);

		if (!npcSettlement.HasStructure(STRUCTURE_TYPE.CITY_CENTER)) {
			StructureSetting cityCenter = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, npcSettlement.owner.factionType.mainResource);
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlementCoroutine(npcSettlement, region.innerMap, cityCenter));
			structuresToPlace.Remove(cityCenter);	
		}

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
				//make structure setting list and unplaced structures list identical so that unplaced structures will try to be placed on next iteration.
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
				// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlementCoroutine(npcSettlement, region.innerMap, structureSetting));
				continue;
			}
			yield return MapGenerator.Instance.StartCoroutine(PlaceStructure(region, structureSetting, npcSettlement));
		}
		yield return null;
	}
	public static IEnumerator PlaceStructure(Region region, StructureSetting structureSetting, NPCSettlement npcSettlement) {
		List<StructureConnector> availableStructureConnectors = npcSettlement.GetStructureConnectorsForStructureType(structureSetting.structureType);
		availableStructureConnectors = CollectionUtilities.Shuffle(availableStructureConnectors);
		List<GameObject> prefabChoices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
		prefabChoices = CollectionUtilities.Shuffle(prefabChoices);
		for (int j = 0; j < prefabChoices.Count; j++) {
			GameObject prefabGO = prefabChoices[j];
			LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
			StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, region.innerMap, out var connectorIndex, out LocationGridTile tileToPlaceStructure, out LocationGridTile connectorTile, structureSetting);
			if (validConnector != null) {
				//instantiate structure object at tile.
				LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, prefabGO, tileToPlaceStructure);
				break; //stop loop since structure was already placed.
			}
		}
		yield return null;
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
				yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsCoroutine());
				
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				List<Character> spawnedCharacters = CreateSettlementResidentsForScenario(dwellingCount, npcSettlement, faction, data, settlementTemplate.minimumVillagerCount);

				//update objects owners in dwellings
				List<TileObject> objectsInDwellings = npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
				for (int j = 0; j < objectsInDwellings.Count; j++) {
					TileObject tileObject = objectsInDwellings[j];
					tileObject.UpdateOwners();
				}
			
				CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);
				// npcSettlement.Initialize();
				yield return null;
			}
			ApplyPreGeneratedCharacterRelationships(data);
			for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++) {
				NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
				settlement.migrationComponent.ForceRandomizePerHourIncrement();
			}
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
	private Faction CreateDefaultFaction(RACE race) {
		FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(race);
		Faction faction = FactionManager.Instance.CreateNewFaction(factionType);
		faction.factionType.SetAsDefault();
		return faction;
	}
	private List<Character> CreateSettlementResidentsForScenario(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data, int providedCitizenCount = -1) {
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			List<Character> createdCharacters = new List<Character>();
			int citizenCount = 0;
			List<string> elfClassesInOrder = new List<string> {"Craftsman", "Craftsman", "Peasant", "Peasant", "Miner", "Miner"};
			List<string> humanClassesPriority = new List<string>() { "Knight", "Shaman", "Knight", "Shaman", "Knight",  "Shaman" };
			GenerateResidentConfiguration(providedCitizenCount, dwellingCount, out var coupleCharacters, out var singleCharacters);
			int neededElves = elfClassesInOrder.Count;
			//spawn couples
			for (int i = 0; i < coupleCharacters; i++) {
				List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
				if (availableDwellings.Count == 0) {
					break; //no more dwellings
				}
				Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
				bool createElves = false;
				if (neededElves >= 2) {
					createElves = GameUtilities.RollChance(50);
				}
				if (createElves) {
					List<Couple> couples = GetAvailableCouplesToBeSpawned(RACE.ELVES, data);
					if (couples.Count > 0) {
						Couple couple = CollectionUtilities.GetRandomElement(couples);
						string class1 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
						elfClassesInOrder.Remove(class1);
						string class2 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
						elfClassesInOrder.Remove(class2);
						createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement, class1, class2));
						citizenCount += 2;
						neededElves -= 2;
					} else {
						//no more couples left	
						List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(RACE.ELVES, data);
						if (siblingCouples.Count > 0) {
							Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
							string class1 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
							elfClassesInOrder.Remove(class1);
							string class2 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
							elfClassesInOrder.Remove(class2);
							createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement, class1, class2));
							citizenCount += 2;
							neededElves -= 2;
						} else {
							//no more sibling Couples	
							PreCharacterData singleCharacter = GetAvailableSingleCharacterForSettlement(RACE.ELVES, data, npcSettlement);
							if (singleCharacter != null) {
								string class1 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
								elfClassesInOrder.Remove(class1);
								createdCharacters.Add(SpawnCharacter(singleCharacter, class1, dwelling, faction, npcSettlement));
								citizenCount += 1;
								neededElves -= 1;
							} else {
								//no more characters to spawn
								Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
								FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(RACE.ELVES);
								DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
								singleCharacter = GetAvailableSingleCharacterForSettlement(RACE.ELVES, data, npcSettlement);
								Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
								string class1 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
								elfClassesInOrder.Remove(class1);
								createdCharacters.Add(SpawnCharacter(singleCharacter, class1, dwelling, faction, npcSettlement));
								citizenCount += 1;
								neededElves -= 1;
							}
						}
					}
				} else {
					//spawn human couple
					string class1 = CollectionUtilities.GetRandomElement(humanClassesPriority);
					humanClassesPriority.Remove(class1);
					string class2 = CollectionUtilities.GetRandomElement(humanClassesPriority);
					humanClassesPriority.Remove(class2);
					CreateCouple(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount, class1, class2);
				}
			}
			
			//spawn singles
			for (int i = 0; i < singleCharacters; i++) {
				List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
				if (availableDwellings.Count == 0) {
					break; //no more dwellings
				}
				Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
				if (neededElves > 0) {
					//spawn an elf
					PreCharacterData singleCharacter = GetAvailableSingleCharacterForSettlement(RACE.ELVES, data, npcSettlement);
					if (singleCharacter != null) {
						string class1 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
						elfClassesInOrder.Remove(class1);
						createdCharacters.Add(SpawnCharacter(singleCharacter, class1, dwelling, faction, npcSettlement));
						citizenCount += 1;
						neededElves -= 1;
					} else {
						//no more characters to spawn
						Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
						FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(RACE.ELVES);
						DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
						singleCharacter = GetAvailableSingleCharacterForSettlement(RACE.ELVES, data, npcSettlement);
						Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
						string class1 = CollectionUtilities.GetRandomElement(elfClassesInOrder);
						elfClassesInOrder.Remove(class1);
						createdCharacters.Add(SpawnCharacter(singleCharacter, class1, dwelling, faction, npcSettlement));
						citizenCount += 1;
						neededElves -= 1;
					}
				} else {
					string class1 = CollectionUtilities.GetRandomElement(humanClassesPriority);
					humanClassesPriority.Remove(class1);
					//spawn human
					CreateSingleCharacter(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount, class1);
				}
			}
			
			return createdCharacters;
		} else {
			return GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data, providedCitizenCount);	
		}
	}
	private void TrySpawnSingleCharacter(NPCSettlement npcSettlement, Faction faction, MapGenerationData data, Dwelling dwelling, ref List<Character> createdCharacters, ref int citizenCount, string className = "") {
		PreCharacterData singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		string classString = string.IsNullOrEmpty(className) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : className;
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
		List<StructureSetting> structures = new List<StructureSetting>(); //{ new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, faction.factionType.mainResource) }; //faction.factionType.GetStructureSettingFor(STRUCTURE_TYPE.CITY_CENTER)
		List<STRUCTURE_TYPE> createdStructureTypes = new List<STRUCTURE_TYPE>();
		for (int i = 0; i < facilityCount; i++) {
			WeightedDictionary<StructureSetting> structuresChoices = GetStructureWeights(createdStructureTypes, faction, settlement.tiles.First());
			StructureSetting chosenSetting = structuresChoices.PickRandomElementGivenWeights();
			structures.Add(chosenSetting);
			createdStructureTypes.Add(chosenSetting.structureType);
		}
		return structures;
	}
	private WeightedDictionary<StructureSetting> GetStructureWeights(List<STRUCTURE_TYPE> structureTypes, Faction faction, HexTile villageCenterTile) {
		WeightedDictionary<StructureSetting> structureWeights = new WeightedDictionary<StructureSetting>();
		List<HexTile> tilesInRange = villageCenterTile.GetTilesInRange(3);
		if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
			if (!structureTypes.Contains(STRUCTURE_TYPE.HOSPICE)) {
				//Apothecary: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HOSPICE, RESOURCE.WOOD), 6); //6
			}
			// structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 15); //1 //Farm: +1
			if (!structureTypes.Contains(STRUCTURE_TYPE.TAVERN)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD), 3);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.CEMETERY) == false) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 2);
			}
			// if (tilesInRange.HasTileWithFeature(TileFeatureDB.Fertile_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 10); //15	
			// }
			if (tilesInRange.HasTileWithFeature(TileFeatureDB.Wood_Source_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD), 15);	
			}
		} else if (faction.factionType.type == FACTION_TYPE.Human_Empire) {
            // structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 15); //1 //Farm: +1
            // if (tilesInRange.HasTileWithFeature(TileFeatureDB.Fertile_Feature)) {
            //     structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 10); //15
            // }
            if (structureTypes.Contains(STRUCTURE_TYPE.MAGE_QUARTERS) == false) {
				//Mage Quarter: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE), 6);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.PRISON) == false) {
				//Prison: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 2); //3
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.BARRACKS) == false) {
				//Barracks: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 6);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.TAVERN) == false) {
				//Barracks: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 3);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.CEMETERY) == false) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 2);
			}
			if (tilesInRange.HasTileWithFeature(TileFeatureDB.Metal_Source_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE), 15);	
			}
			// if (tilesInRange.HasTileWithFeature(TileFeatureDB.Game_Feature)) {
			// 	structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE), 15);	
			// }
		}
		return structureWeights;
	}
	#endregion

	#region Residents
	private void GenerateResidentConfiguration(int providedCitizenCount, int dwellingCount, out int coupleCharacters, out int singleCharacters) {
		singleCharacters = 0;
		coupleCharacters = 0;
		if (providedCitizenCount != -1 && dwellingCount < providedCitizenCount) {
			int remainingCharacters = providedCitizenCount;
			int remainingDwellings = dwellingCount;
			for (int i = 0; i < dwellingCount; i++) {
				if (remainingDwellings >= remainingCharacters) {
					singleCharacters++;
					remainingCharacters -= 1;
				}
				else {
					coupleCharacters++;
					remainingCharacters -= 2;
				}
				remainingDwellings--;
			}
		} else {
			for (int i = 0; i < dwellingCount; i++) {
				if (GameUtilities.RollChance(35)) {
					coupleCharacters++;
				} else {
					singleCharacters++;
				}
				if (providedCitizenCount > 0) {
					int totalCharacters = singleCharacters + (coupleCharacters * 2);
					if (totalCharacters >= providedCitizenCount) {
						break;
					}
				}
			}
		}
	}
	private List<Character> GenerateSettlementResidents(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data, int providedCitizenCount = -1) {
		GenerateResidentConfiguration(providedCitizenCount, dwellingCount, out var coupleCharacters, out var singleCharacters);
		Debug.Log($"Provided citizen count is {providedCitizenCount.ToString()}. Singles: {singleCharacters.ToString()}. Couples: {coupleCharacters.ToString()}");
		
		List<Character> createdCharacters = new List<Character>();
		int citizenCount = 0;
		
		//spawn couples
		for (int i = 0; i < coupleCharacters; i++) {
			List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellings.Count == 0) {
				break; //no more dwellings
			}
			Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
			CreateCouple(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount);
		}
		
		//spawn singles
		for (int i = 0; i < singleCharacters; i++) {
			List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellings.Count == 0) {
				break; //no more dwellings
			}
			Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
			CreateSingleCharacter(npcSettlement, faction, data, dwelling, ref createdCharacters, ref citizenCount);
		}
		
		
		return createdCharacters;
	}
	private void CreateSingleCharacter(NPCSettlement npcSettlement, Faction faction, MapGenerationData data, Dwelling dwelling, ref List<Character> createdCharacters, ref int citizenCount, string providedClass = "") {
		//single
		PreCharacterData singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		if (singleCharacter != null) {
			createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : providedClass, dwelling, faction, npcSettlement));
			citizenCount += 1;
		}
		else {
			//no more characters to spawn
			Debug.LogWarning("Could not find any more characters to spawn");
			FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
			DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
			singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
			Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
			createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : providedClass, dwelling, faction, npcSettlement));
			citizenCount += 1;
		}
	}
	private void CreateCouple(NPCSettlement npcSettlement, Faction faction, MapGenerationData data, Dwelling dwelling, ref List<Character> createdCharacters, ref int citizenCount, string providedClass1 = "", string providedClass2 = "") {
		List<Couple> couples = GetAvailableCouplesToBeSpawned(faction.race, data);
		if (couples.Count > 0) {
			Couple couple = CollectionUtilities.GetRandomElement(couples);
			createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement, providedClass1, providedClass2));
			citizenCount += 2;
		} else {
			//no more couples left	
			List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
			if (siblingCouples.Count > 0) {
				Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
				createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement, providedClass1, providedClass2));
				citizenCount += 2;
			}
			else {
				//no more sibling Couples	
				PreCharacterData singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
				if (singleCharacter != null) {
					createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass1) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : providedClass1, dwelling, faction, npcSettlement));
					citizenCount += 1;
				}
				else {
					//no more characters to spawn
					Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
					FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
					DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
					singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
					Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
					createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass1) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : providedClass1, dwelling, faction, npcSettlement));
					citizenCount += 1;
				}
			}
		}
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
			SpawnCharacter(couple.character1, string.IsNullOrEmpty(className1) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : className1, dwelling, faction, npcSettlement),
			SpawnCharacter(couple.character2, string.IsNullOrEmpty(className2) ? npcSettlement.settlementClassTracker.GetNextClassToCreateAndIncrementOrder(faction) : className2, dwelling, faction, npcSettlement)	
		};
		return characters;
	}
	private Character SpawnCharacter(PreCharacterData data, string className, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement) {
		return CharacterManager.Instance.CreateNewCharacter(data, className, faction, npcSettlement, dwelling);
	}
	#endregion

	#region Relationships
	private void ApplyPreGeneratedCharacterRelationships(MapGenerationData data) {
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
				return LOCATION_TYPE.VILLAGE;
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