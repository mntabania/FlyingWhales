using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class VillageGeneration : MapGenerationComponent {

#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating Settlements...");
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
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
		List<NPCSettlement> createdSettlements = RuinarchListPool<NPCSettlement>.Claim();
		List<VillageSetting> villageSettings = RuinarchListPool<VillageSetting>.Claim();
		//Generate Settlements and Factions with city centers
		foreach (var setting in data.determinedVillages) {
			FactionTemplate factionTemplate = setting.Key;
			RACE race;
			if (factionTemplate.factionType == FACTION_TYPE.Elven_Kingdom) {
				race = RACE.ELVES;
			} else if (factionTemplate.factionType == FACTION_TYPE.Human_Empire) {
				race = RACE.HUMANS;
			} else {
				race = GameUtilities.RollChance(50) ? RACE.ELVES : RACE.HUMANS;
			}
			Faction faction = FactionManager.Instance.CreateNewFaction(factionTemplate.factionType, factionTemplate.name, factionTemplate.factionEmblem, race);
			faction.factionType.SetAsDefault();
			LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
			for (int i = 0; i < setting.Value.Count; i++) {
				VillageSpot villageSpot = setting.Value[i]; 
				Area settlementTile = villageSpot.mainSpot;
				VillageSetting villageSetting = factionTemplate.villageSettings[i];
				NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, settlementTile);
				npcSettlement.SetOccupiedVillageSpot(villageSpot);
				createdSettlements.Add(npcSettlement);
				villageSettings.Add(villageSetting);
				
				npcSettlement.SetName(villageSetting.villageName);
				LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
				SETTLEMENT_TYPE settlementType = LandmarkManager.Instance.GetSettlementTypeForFaction(faction);
				npcSettlement.SetSettlementType(settlementType);
				
				var structureSettings = GenerateCityCenter(faction, villageSetting, npcSettlement);
				
				Assert.IsTrue(structureSettings.First().structureType == STRUCTURE_TYPE.CITY_CENTER);
				Assert.IsTrue(npcSettlement.areas.Count > 0);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
		}
		
		//Generate Dwellings
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting villageSetting = villageSettings[i];
			var structureSettings = GenerateDwellings(npcSettlement.owner, villageSetting, npcSettlement);
			yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
			RuinarchListPool<StructureSetting>.Release(structureSettings);
		}
		
		//Generate facilities and residents
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting villageSetting = villageSettings[i];
			var structureSettings = GenerateFacilities(npcSettlement, npcSettlement.owner, villageSetting.GetRandomFacilityCount());
			Debug.Log($"Will create facilities for {npcSettlement.name}: {structureSettings.ComafyList()}");
			yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement));
			yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsCoroutine());

			if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, npcSettlement.owner, data);
				List<TileObject> objectsInDwellings = RuinarchListPool<TileObject>.Claim();
				npcSettlement.PopulateTileObjectsFromStructures<TileObject>(objectsInDwellings, STRUCTURE_TYPE.DWELLING);
				for (int j = 0; j < objectsInDwellings.Count; j++) {
					TileObject tileObject = objectsInDwellings[j];
					tileObject.UpdateOwners();
				}
				RuinarchListPool<TileObject>.Release(objectsInDwellings);
				CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);	
			}
		}
		RuinarchListPool<NPCSettlement>.Release(createdSettlements);
		RuinarchListPool<VillageSetting>.Release(villageSettings);
	}
	private List<StructureSetting> GenerateCityCenter(Faction p_faction, VillageSetting p_villageSetting, NPCSettlement p_settlement) {
		List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
		structureSettings.Add(new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, p_faction.factionType.mainResource, p_faction.factionType.usesCorruptedStructures));
		return structureSettings;
	}
	private List<StructureSetting> GenerateDwellings(Faction p_faction, VillageSetting p_villageSetting, NPCSettlement p_settlement) {
		List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
		int randomDwellings = p_villageSetting.GetRandomDwellingCount();
		var dwellingSetting = p_settlement.settlementType.GetDwellingSetting(p_faction);
		for (int i = 0; i < randomDwellings; i++) {
			structureSettings.Add(dwellingSetting);
		}
		return structureSettings;
	}
	private IEnumerator EnsuredStructurePlacement(Region region, List<StructureSetting> structureSettings, NPCSettlement npcSettlement) {
		List<StructureSetting> unplacedStructures = new List<StructureSetting>();
		List<StructureSetting> structuresToPlace = new List<StructureSetting>(structureSettings);

		if (!npcSettlement.HasStructure(STRUCTURE_TYPE.CITY_CENTER)) {
			StructureSetting cityCenter = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, npcSettlement.owner.factionType.mainResource, npcSettlement.owner.factionType.usesCorruptedStructures);
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceFirstStructureForSettlement(npcSettlement, region.innerMap, cityCenter));
			structuresToPlace.Remove(cityCenter);	
		}

		for (int i = 0; i < 4; i++) {
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

#if DEBUG_LOG
				if (i + 1 == 2) {
					//last iteration
					string summary = $"Was unable to place the following structures:";
					for (int j = 0; j < unplacedStructures.Count; j++) {
						summary = $"{summary}\n- {unplacedStructures[j].ToString()}";
					}
					Debug.Log(summary);
				}
#endif
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
		List<StructureConnector> availableStructureConnectors = RuinarchListPool<StructureConnector>.Claim();
		npcSettlement.PopulateStructureConnectorsForStructureType(availableStructureConnectors, structureSetting.structureType);
		// availableStructureConnectors = CollectionUtilities.Shuffle(availableStructureConnectors);
		List<GameObject> prefabChoices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
		CollectionUtilities.Shuffle(prefabChoices);
		for (int j = 0; j < prefabChoices.Count; j++) {
			GameObject prefabGO = prefabChoices[j];
			LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
			StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, region.innerMap, out var connectorIndex, out LocationGridTile tileToPlaceStructure, out LocationGridTile connectorTile, structureSetting);
			if (validConnector != null) {
				//instantiate structure object at tile.
				LocationStructure structure =  LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, prefabGO, tileToPlaceStructure);
				if (structure is ManMadeStructure mmStructure) {
					mmStructure.OnUseStructureConnector(connectorTile);    
				}
				break; //stop loop since structure was already placed.
			} else {
				Debug.LogWarning($"Could not find structure connector for {prefabObject.name}. Choices are:\n{availableStructureConnectors.ComafyList()}");
			}
		}
		RuinarchListPool<StructureConnector>.Release(availableStructureConnectors);
		yield return null;
	}
#endregion

#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		if (scenarioMapData.villageSettlementTemplates != null) {
			for (int i = 0; i < scenarioMapData.villageSettlementTemplates.Length; i++) {
				SettlementTemplate settlementTemplate = scenarioMapData.villageSettlementTemplates[i];
				Area[] tilesInSettlement = settlementTemplate.GetTilesInTemplate(GridMap.Instance.map);

				Region region = tilesInSettlement[0].region;

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
				List<TileObject> objectsInDwellings = RuinarchListPool<TileObject>.Claim();
				npcSettlement.PopulateTileObjectsFromStructures<TileObject>(objectsInDwellings, STRUCTURE_TYPE.DWELLING);
				for (int j = 0; j < objectsInDwellings.Count; j++) {
					TileObject tileObject = objectsInDwellings[j];
					tileObject.UpdateOwners();
				}
				RuinarchListPool<TileObject>.Release(objectsInDwellings);
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
			WeightedDictionary<StructureSetting> structuresChoices = GetStructureWeights(createdStructureTypes, faction, settlement.areas.First(), settlement);
			if (structuresChoices.GetTotalOfWeights() > 0) {
				StructureSetting chosenSetting = structuresChoices.PickRandomElementGivenWeights();
				structures.Add(chosenSetting);
				createdStructureTypes.Add(chosenSetting.structureType);	
			}
		}
		return structures;
	}
	private WeightedDictionary<StructureSetting> GetStructureWeights(List<STRUCTURE_TYPE> structureTypes, Faction faction, Area villageCenterTile, NPCSettlement settlement) {
		WeightedDictionary<StructureSetting> structureWeights = new WeightedDictionary<StructureSetting>();
		List<Area> tilesInRange = RuinarchListPool<Area>.Claim();
		villageCenterTile.PopulateAreasInRange(tilesInRange, 3);
		if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || settlement.settlementType.settlementType == SETTLEMENT_TYPE.Elven_Hamlet) {
			if (!structureTypes.Contains(STRUCTURE_TYPE.HOSPICE)) {
				//Apothecary: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HOSPICE, RESOURCE.WOOD), 6);
			}
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 1);
			if (!structureTypes.Contains(STRUCTURE_TYPE.TAVERN)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD), 3);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.CEMETERY)) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.FARM)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 15);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Fish_Source) && !structureTypes.Contains(STRUCTURE_TYPE.FISHING_SHACK)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FISHING_SHACK, RESOURCE.WOOD), 5);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Wood_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD), 40);	
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Metal_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.MINE_SHACK)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.WOOD), 2);	
			}
		} else if (faction.factionType.type == FACTION_TYPE.Human_Empire || settlement.settlementType.settlementType == SETTLEMENT_TYPE.Human_Village) {
			if (!structureTypes.Contains(STRUCTURE_TYPE.MAGE_QUARTERS)) {
				//Mage Quarter: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE), 6);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.PRISON)) {
				//Prison: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.BARRACKS)) {
				//Barracks: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 6);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.TAVERN)) {
				//Barracks: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 3);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.CEMETERY)) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.FARM)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 15);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Fish_Source) && !structureTypes.Contains(STRUCTURE_TYPE.FISHING_SHACK)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FISHING_SHACK, RESOURCE.STONE), 5);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Metal_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.MINE_SHACK)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE), 40);	
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Game_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE), 15);	
			}
		} else {
			if (!structureTypes.Contains(STRUCTURE_TYPE.PRISON)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.PRISON, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.BARRACKS)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.BARRACKS, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 6);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.TAVERN)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 3);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Metal_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.MINE_SHACK)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 10);	
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Wood_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 10);	
			}
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HOSPICE, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 6);
		}
		RuinarchListPool<Area>.Release(tilesInRange);
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
			if (dwellingCount == 2) {
				//this is to prevent only 2 characters spawning in custom if only 2 dwellings were placed
				coupleCharacters = 2;
				singleCharacters = 0;
				return;
			}
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
#if DEBUG_LOG
		Debug.Log($"Provided citizen count is {providedCitizenCount.ToString()}. Singles: {singleCharacters.ToString()}. Couples: {coupleCharacters.ToString()}");
#endif
		
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
		return CharacterManager.Instance.CreateNewCharacter(data, className, faction, npcSettlement, dwelling, afterInitializationAction: (character) => AfterCharacterInitializationProcess(character, faction));
	}
	private void AfterCharacterInitializationProcess(Character p_character, Faction p_faction) {
		if (p_faction.factionType.type == FACTION_TYPE.Demon_Cult) {
			//Make sure that characters religion is Demon Worship if he/she is to be part of a Demon Cult. This is to ensure compatibility with Demon Cult
			//since that faction type is exclusive to Demon Worshippers.
			p_character.religionComponent.ChangeReligion(RELIGION.Demon_Worship);
		}
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
	private LOCATION_TYPE GetLocationTypeForRace(RACE race) {
		switch (race) {
			case RACE.HUMANS:
			case RACE.ELVES:
				return LOCATION_TYPE.VILLAGE;
			default:
				throw new Exception($"There was no location type provided for race {race.ToString()}");
		}
	}
#endregion
	
}