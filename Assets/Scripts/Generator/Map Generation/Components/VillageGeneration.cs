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
			if (!succeess) {
				yield break;
			}
			
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
				Assert.IsNotNull(villageSpot);
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
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
		}

		//Generate Dwellings
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting villageSetting = villageSettings[i];
			var structureSettings = GenerateDwellings(npcSettlement.owner, villageSetting, npcSettlement);
			int neededDwellingCount = structureSettings.Count;
			yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
			RuinarchListPool<StructureSetting>.Release(structureSettings);
			if (npcSettlement.GetStructureCount(STRUCTURE_TYPE.DWELLING) < neededDwellingCount) {
				succeess = false; //failed to generate needed amount of dwellings
				yield break;
			}
		}

		//generate residents
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, npcSettlement.owner, data);

				RandomizeCharacterClassesBasedOnTalents(npcSettlement, spawnedCharacters);
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

		//generate food producers
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting villageSetting = villageSettings[i];
			int neededFoodProducingStructures = villageSetting.GetFoodProducingStructureCount();
			var missingFoodProducers = 0;
			for (int j = 0; j < neededFoodProducingStructures; j++) {
				bool wasStructurePlaced = false;
				List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
				StructureSetting structureToPlace;
				if (ShouldBuildFishery(npcSettlement)) {
					structureToPlace = new StructureSetting(STRUCTURE_TYPE.FISHERY, RESOURCE.WOOD, npcSettlement.owner.factionType.usesCorruptedStructures);
					structureSettings.Add(structureToPlace);
					yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
					if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count <= 0) {
						//fishery was placed successfully
						Debug.Log($"Fishery was placed successfully for {npcSettlement.name}!");
						wasStructurePlaced = true;
					}
				}
				if (!wasStructurePlaced) {
					if (ShouldBuildButcher(npcSettlement)) {
						structureSettings.Clear();
						structureToPlace = new StructureSetting(STRUCTURE_TYPE.BUTCHERS_SHOP, RESOURCE.STONE, npcSettlement.owner.factionType.usesCorruptedStructures);
						structureSettings.Add(structureToPlace);
						yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
						if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count <= 0) {
							//butcher was placed successfully
							Debug.Log($"Butcher was placed successfully for {npcSettlement.name}!");
							wasStructurePlaced = true;
						}
					}
				}
				
				if(!wasStructurePlaced) {
					structureSettings.Clear();
					structureToPlace = new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE, npcSettlement.owner.factionType.usesCorruptedStructures);
					structureSettings.Add(structureToPlace);
					yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
					if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count <= 0) {
						//farm was placed successfully
						Debug.Log($"Farm was placed successfully for {npcSettlement.name}!");
						wasStructurePlaced = true;
					}
				}

				if (!wasStructurePlaced) {
					//was unable to place any food producing structure
					missingFoodProducers++;
					Debug.Log($"Was unable to place a food producing structure. Missing Food Structures are: {missingFoodProducers}");	
				}
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
			data.SetMissingFoodProducers(npcSettlement, missingFoodProducers);
		}

		//generate basic resource structures
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting villageSetting = villageSettings[i];
			int neededBasicResourceProducingStructures = villageSetting.GetBasicResourceProducingStructureCount();
			var missingBasicResourceProducers = 0;
			for (int j = 0; j < neededBasicResourceProducingStructures; j++) {
				List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
				StructureSetting structureToPlace;
				if (npcSettlement.owner.factionType.type == FACTION_TYPE.Human_Empire) {
					structureToPlace = new StructureSetting(STRUCTURE_TYPE.MINE, RESOURCE.NONE);
				} else if (npcSettlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom) {
					structureToPlace = new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE);
				} else {
					structureToPlace = GameUtilities.RollChance(50) ? new StructureSetting(STRUCTURE_TYPE.MINE, RESOURCE.NONE) : new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE);
				}
				structureSettings.Add(structureToPlace);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
				if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall != null && data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count > 0) {
					missingBasicResourceProducers += data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count;
					Debug.Log($"Was unable to place {data.unplacedStructuresOnLastEnsuredStructurePlacementCall.ComafyList()}. Missing Basic Resource Structures are: {missingBasicResourceProducers}");
				}
				RuinarchListPool<StructureSetting>.Release(structureSettings);
				data.SetMissingBasicResourceProducers(npcSettlement, missingBasicResourceProducers);
			}
		}
		
		List<STRUCTURE_TYPE> specialStructureTypes = RuinarchListPool<STRUCTURE_TYPE>.Claim();

		//Generate special structures
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting villageSetting = villageSettings[i];
			int neededSpecialStructures = villageSetting.GetSpecialStructureCount();
			int additionalSpecialStructures = data.GetTotalMissingProductionStructures(npcSettlement);
			int totalSpecialStructures = neededSpecialStructures + additionalSpecialStructures;
			
			specialStructureTypes.Clear();
			specialStructureTypes.Add(STRUCTURE_TYPE.PRISON);
			specialStructureTypes.Add(STRUCTURE_TYPE.CEMETERY);
			specialStructureTypes.Add(STRUCTURE_TYPE.TAVERN);
			specialStructureTypes.Add(STRUCTURE_TYPE.HOSPICE);
			specialStructureTypes.Add(STRUCTURE_TYPE.WORKSHOP);
			
			Debug.Log($"Will generate {totalSpecialStructures.ToString()} for {npcSettlement.name}");
			for (int j = 0; j < totalSpecialStructures; j++) {
				if (specialStructureTypes.Count <= 0) { break; }
				STRUCTURE_TYPE chosenStructureType = CollectionUtilities.GetRandomElement(specialStructureTypes);
				specialStructureTypes.Remove(chosenStructureType);
				List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
				StructureSetting structureSetting = npcSettlement.owner.factionType.CreateStructureSettingForStructure(chosenStructureType, npcSettlement);
				structureSettings.Add(structureSetting);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
		}
		
		//Generate Initial Objects
		for (int i = 0; i < createdSettlements.Count; i++) {
			NPCSettlement npcSettlement = createdSettlements[i];
			yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsForWorldGenCoroutine());
		}
		
		// //Generate facilities
		// for (int i = 0; i < createdSettlements.Count; i++) {
		// 	NPCSettlement npcSettlement = createdSettlements[i];
		// 	VillageSetting villageSetting = villageSettings[i];
		// 	var structureSettings = GenerateFacilities(npcSettlement, npcSettlement.owner, villageSetting.GetFacilityCount());
		// 	Debug.Log($"Will create facilities for {npcSettlement.name}: {structureSettings.ComafyList()}");
		// 	yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings, npcSettlement, data));
		// 	yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsCoroutine());
		// }
		
		RuinarchListPool<NPCSettlement>.Release(createdSettlements);
		RuinarchListPool<VillageSetting>.Release(villageSettings);
	}
	private bool ShouldBuildFishery(NPCSettlement p_settlement) {
		if (p_settlement.HasStructure(STRUCTURE_TYPE.FISHERY)) {
			return false;
		}
		if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
			//Animal Killing is considered a crime.
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasUnusedFishingSpot()) {
			return false;
		}
		if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Fisher")) {
			return false;
		}
		return true;
	}
	private bool ShouldBuildButcher(NPCSettlement p_settlement) {
		if (p_settlement.HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP)) {
			return false;
		}
		if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
			//Animal Killing is considered a crime.
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasAccessToButcherAnimals()) {
			return false;
		}
		if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Butcher")) {
			return false;
		}
		return true;
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
	private IEnumerator EnsuredStructurePlacement(Region region, List<StructureSetting> structureSettings, NPCSettlement npcSettlement, MapGenerationData p_data) {
		List<StructureSetting> unplacedStructures = new List<StructureSetting>();
		List<StructureSetting> structuresToPlace = new List<StructureSetting>(structureSettings);

		if (!npcSettlement.HasStructure(STRUCTURE_TYPE.CITY_CENTER)) {
			StructureSetting cityCenter = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, npcSettlement.owner.factionType.mainResource, npcSettlement.owner.factionType.usesCorruptedStructures);
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceFirstStructureForSettlement(npcSettlement, region.innerMap, cityCenter));
			structuresToPlace.Remove(cityCenter);	
		}

		for (int i = 0; i < 4; i++) {
			yield return MapGenerator.Instance.StartCoroutine(PlaceStructures(region, structuresToPlace, npcSettlement, p_data));
			//check whole structure list to verify if all needed structures were placed.
			unplacedStructures.Clear();
			unplacedStructures.AddRange(structuresToPlace);
			// unplacedStructures.AddRange(structureSettings);
			// for (int j = 0; j < npcSettlement.allStructures.Count; j++) {
			// 	LocationStructure structure = npcSettlement.allStructures[j];
			// 	if (structure is ManMadeStructure manMadeStructure) {
			// 		for (int k = 0; k < unplacedStructures.Count; k++) {
			// 			StructureSetting structureSetting = unplacedStructures[k];
			// 			if (manMadeStructure.structureType == structureSetting.structureType) {
			// 				//&& manMadeStructure.wallsAreMadeOf == structureSetting.resource
			// 				unplacedStructures.RemoveAt(k);
			// 				break;
			// 			}
			// 		}
			// 	}
			// }
			
			//check last placed structures against unplaced structures, if a structure type was placed,
			//remove an entry from the unplaced structures list, this will ensure that only structure types that weren't placed
			//are kept in the unplacedStructures list
			for (int j = 0; j < p_data.LastPlacedStructureTypes.Count; j++) {
				STRUCTURE_TYPE structureType = p_data.LastPlacedStructureTypes[j];
				for (int k = 0; k < unplacedStructures.Count; k++) {
					StructureSetting structureSetting = unplacedStructures[k];
					if (structureType == structureSetting.structureType) {
						unplacedStructures.RemoveAt(k);
						break;
					}
				}
			}
			if (unplacedStructures.Count == 0) {
				break; //no more unplaced structures
			} else {
				//make structure setting list and unplaced structures list identical so that unplaced structures will try to be placed on next iteration.
				structuresToPlace.Clear();
				structuresToPlace.AddRange(unplacedStructures);
#if DEBUG_LOG
				if (i + 1 == 4) {
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
		p_data.SetLastUnplacedStructures(unplacedStructures);
	}
	private IEnumerator PlaceStructures(Region region, List<StructureSetting> structureSettings, NPCSettlement npcSettlement, MapGenerationData p_data) {
		p_data.ClearLastPlacedVillageStructures();
		for (int i = 0; i < structureSettings.Count; i++) {
			StructureSetting structureSetting = structureSettings[i];
			if (structureSetting.structureType == STRUCTURE_TYPE.CITY_CENTER) {
				// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlementCoroutine(npcSettlement, region.innerMap, structureSetting));
				continue;
			}
			yield return MapGenerator.Instance.StartCoroutine(PlaceStructure(region, structureSetting, npcSettlement, p_data));
		}
		yield return null;
	}

	private IEnumerator PlaceStructure(Region region, StructureSetting structureToPlace, NPCSettlement npcSettlement, MapGenerationData p_data) {
		List<StructureConnector> availableStructureConnectors = RuinarchListPool<StructureConnector>.Claim();
		npcSettlement.PopulateStructureConnectorsForStructureType(availableStructureConnectors, structureToPlace.structureType);
		
		// string connectorLog;
		StructureConnector validConnector;
		string structurePrefabName;
		LocationGridTile connectorTile;
		LocationGridTile targetTile;
		
		if (structureToPlace.structureType == STRUCTURE_TYPE.MINE) {
			//order spots based on distance with settlement city center
			availableStructureConnectors = availableStructureConnectors.OrderBy(c => Vector2.Distance(c.transform.position, 
				npcSettlement.cityCenter.tiles.ElementAt(0).centeredWorldLocation)).ToList();
// #if DEBUG_LOG
// 			Debug.Log($"Evaluating structure connectors for {npcSettlement.name} to place {structureToPlace.ToString()}. Available connectors are:\n {availableStructureConnectors.ComafyList()}");
// #endif
			validConnector = LandmarkManager.Instance.CanPlaceStructureBlueprintMine(npcSettlement, structureToPlace, availableStructureConnectors, out targetTile, 
				out structurePrefabName, out var connectorToUse, out connectorTile, out var canPlace, out _);
// #if DEBUG_LOG
// 			Debug.Log($"Found Connector at {validConnector}. Connector Log for {npcSettlement.name} to place {structureToPlace.ToString()}:\n {connectorLog}");
// #endif
		} else {
			//did not shuffle connectors for mine since we want the village to place the mine as close as possible.
			//Related card: https://trello.com/c/lFTbmJ4d/4932-optimize-mine-placement
			CollectionUtilities.Shuffle(availableStructureConnectors);
// #if DEBUG_LOG
// 			Debug.Log($"Evaluating structure connectors for {npcSettlement.name} to place {structureToPlace.ToString()}. Available connectors are:\n {availableStructureConnectors.ComafyList()}");
// #endif
			validConnector = LandmarkManager.Instance.CanPlaceStructureBlueprintDefault(npcSettlement, structureToPlace, availableStructureConnectors, out targetTile, 
				out structurePrefabName, out var connectorToUse, out connectorTile, out var canPlace, out _);
// #if DEBUG_LOG
// 			Debug.Log($"Found Connector at {validConnector}. Connector Log for {npcSettlement.name} to place {structureToPlace.ToString()}:\n {connectorLog}");
// #endif
		}
		
		if (validConnector != null) {
			//instantiate structure object at tile.
			GameObject prefabGO = ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName);
			LocationStructure structure =  LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, prefabGO, targetTile);
			if (structure is ManMadeStructure mmStructure) {
				mmStructure.OnUseStructureConnector(connectorTile);    
			}
			p_data.AddLastPlacedStructureTypes(structure.structureType);
		}
		
		// List<GameObject> prefabChoices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
		// CollectionUtilities.Shuffle(prefabChoices);
		// for (int j = 0; j < prefabChoices.Count; j++) {
		// 	GameObject prefabGO = prefabChoices[j];
		// 	LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
		// 	StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, region.innerMap, npcSettlement, out var connectorIndex, 
		// 		out LocationGridTile tileToPlaceStructure, out LocationGridTile connectorTile, structureSetting, out var functionLog);
		// 	if (validConnector != null) {
		// 		//instantiate structure object at tile.
		// 		LocationStructure structure =  LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, prefabGO, tileToPlaceStructure);
		// 		if (structure is ManMadeStructure mmStructure) {
		// 			mmStructure.OnUseStructureConnector(connectorTile);    
		// 		}
		// 		p_data.AddLastPlacedStructureTypes(structure.structureType);
		// 		break; //stop loop since structure was already placed.
		// 	} else {
		// 		Debug.LogWarning($"Could not find structure connector for {prefabObject.name}. Choices are:\n{availableStructureConnectors.ComafyList()}. Connector Summaries:\n{functionLog}");
		// 	}
		// }
		RuinarchListPool<StructureConnector>.Release(availableStructureConnectors);
		yield return null;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		if (scenarioMapData.villageSettlementTemplates != null) {
			List<string> civilianClassOrder = RuinarchListPool<string>.Claim();
			string foodProducer = "Food Producer";
			string basicResourceProducer = "Basic Resource Producer";
			string specialCivilian = "Special Civilian";
			civilianClassOrder.Add(foodProducer);
			civilianClassOrder.Add(basicResourceProducer);
			civilianClassOrder.Add(specialCivilian);
			for (int i = 0; i < scenarioMapData.villageSettlementTemplates.Length; i++) {
				SettlementTemplate settlementTemplate = scenarioMapData.villageSettlementTemplates[i];
				Area[] tilesInSettlement = settlementTemplate.GetTilesInTemplate(GridMap.Instance.map);

				Region region = tilesInSettlement[0].region;

				//create faction
				Faction faction = GetFactionForScenario(settlementTemplate);

				LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);

				Area first = tilesInSettlement.First();
				NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, first);
				VillageSpot villageSpot = GridMap.Instance.mainRegion.GetVillageSpotOnArea(first);
				Assert.IsNotNull(villageSpot);
				npcSettlement.SetOccupiedVillageSpot(villageSpot);
				npcSettlement.SetSettlementType(settlementTemplate.settlementType);
				// npcSettlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
				LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
				
				StructureSetting[] structureSettings = settlementTemplate.structureSettings;
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, structureSettings.ToList(), npcSettlement, data));
				// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltStructuresForSettlement(npcSettlement, region.innerMap, structureSettings));
				yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjectsForWorldGenCoroutine());
				
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				List<Character> spawnedCharacters = CreateSettlementResidentsForScenario(dwellingCount, npcSettlement, faction, data, settlementTemplate.minimumVillagerCount);

				if (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Pangat_Loo) { //do not randomize classes in pangat loo since it has classes pre set.
					RandomizeCharacterClassesBasedOnTalents(npcSettlement, spawnedCharacters);
				}
				
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
			RuinarchListPool<string>.Release(civilianClassOrder);
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
			List<string> elfClassesInOrder = new List<string> {"Crafter", "Crafter", "Farmer", "Farmer", "Miner", "Miner"};
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
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE), 1);
			if (!structureTypes.Contains(STRUCTURE_TYPE.TAVERN)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD), 3);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.CEMETERY)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.FARM)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE), 15);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Fish_Source) && !structureTypes.Contains(STRUCTURE_TYPE.FISHERY)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FISHERY, RESOURCE.WOOD), 5);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Wood_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE), 40);	
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Metal_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.MINE)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE, RESOURCE.NONE), 2);	
			}
		} else if (faction.factionType.type == FACTION_TYPE.Human_Empire || settlement.settlementType.settlementType == SETTLEMENT_TYPE.Human_Village) {
			if (!structureTypes.Contains(STRUCTURE_TYPE.MAGE_QUARTERS)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE), 6);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.PRISON)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.BARRACKS)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 6);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.TAVERN)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 3);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.CEMETERY)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 2);
			}
			if (!structureTypes.Contains(STRUCTURE_TYPE.FARM)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE), 15);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Fish_Source) && !structureTypes.Contains(STRUCTURE_TYPE.FISHERY)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FISHERY, RESOURCE.WOOD), 5);
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Metal_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.MINE)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE, RESOURCE.NONE), 40);	
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Wood_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE), 2);	
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
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Metal_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.MINE)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE, faction.factionType.usesCorruptedStructures), 10);	
			}
			if (settlement.HasReservedSpotWithFeature(AreaFeatureDB.Wood_Source_Feature) && !structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE, faction.factionType.usesCorruptedStructures), 10);	
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
			createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass) ? "Farmer" : providedClass, dwelling, faction, npcSettlement));
			citizenCount += 1;
		}
		else {
			//no more characters to spawn
			Debug.LogWarning("Could not find any more characters to spawn");
			FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
			DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
			singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
			Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
			createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass) ? "Farmer" : providedClass, dwelling, faction, npcSettlement));
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
					createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass1) ? "Farmer" : providedClass1, dwelling, faction, npcSettlement));
					citizenCount += 1;
				}
				else {
					//no more characters to spawn
					Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
					FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
					DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(newFamily);
					singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
					Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
					createdCharacters.Add(SpawnCharacter(singleCharacter, string.IsNullOrEmpty(providedClass1) ? "Farmer" : providedClass1, dwelling, faction, npcSettlement));
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
			SpawnCharacter(couple.character1, string.IsNullOrEmpty(className1) ? "Farmer" : className1, dwelling, faction, npcSettlement),
			SpawnCharacter(couple.character2, string.IsNullOrEmpty(className2) ? "Farmer" : className2, dwelling, faction, npcSettlement)	
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
	private void RandomizeCharacterClassesBasedOnTalents(NPCSettlement npcSettlement, List<Character> spawnedCharacters) {
		List<string> civilianClassOrder = RuinarchListPool<string>.Claim();
		string foodProducer = "Food Producer";
		string basicResourceProducer = "Basic Resource Producer";
		string specialCivilian = "Special Civilian";
		civilianClassOrder.Add(foodProducer);
		civilianClassOrder.Add(basicResourceProducer);
		civilianClassOrder.Add(specialCivilian);
#if DEBUG_LOG
		string summary = $"Setting initial classes of {npcSettlement.name} residents ({spawnedCharacters.Count.ToString()})";
#endif
		List<Character> charactersToChangeClass = RuinarchListPool<Character>.Claim();
		charactersToChangeClass.AddRange(spawnedCharacters);
		//change class of spawned characters.
		//combatants
		int neededCombatants = SettlementClassComponent.GetNumberOfNeededCombatants(spawnedCharacters.Count);
#if DEBUG_LOG
		summary = $"{summary}\nGenerating {neededCombatants.ToString()} Combatants";
#endif
		for (int j = 0; j < neededCombatants; j++) {
			Character chosenCharacter = CollectionUtilities.GetRandomElement(charactersToChangeClass);
			charactersToChangeClass.Remove(chosenCharacter);
			List<string> ableCombatantClasses = RuinarchListPool<string>.Claim();
			chosenCharacter.classComponent.PopulateAbleCombatantClasses(ableCombatantClasses);
			string chosenClass = CollectionUtilities.GetRandomElement(ableCombatantClasses);
			chosenCharacter.classComponent.AssignClass(chosenClass, true);
			chosenCharacter.classComponent.OnUpdateCharacterClass();
#if DEBUG_LOG
			summary = $"{summary}\n\tSet class of {chosenCharacter.name} to {chosenClass}";
#endif
			RuinarchListPool<string>.Release(ableCombatantClasses);
		}

		//civilians
		int civilianCount = spawnedCharacters.Count - neededCombatants;
#if DEBUG_LOG
		summary = $"{summary}\nGenerating {civilianCount.ToString()} Civilians";
#endif
		int currentCivilianOrderIndex = 0;
		for (int j = 0; j < civilianCount; j++) {
			Character chosenCharacter = CollectionUtilities.GetRandomElement(charactersToChangeClass);
			charactersToChangeClass.Remove(chosenCharacter);

			//produce civilians based on civilianClassOrder.
			//This will iterate through that list for each successful loop of this
			string chosenClassType = civilianClassOrder[currentCivilianOrderIndex];
			currentCivilianOrderIndex++;
			if (currentCivilianOrderIndex >= civilianClassOrder.Count) {
				currentCivilianOrderIndex = 0;
			}

			List<string> civilianClassChoices = RuinarchListPool<string>.Claim();
			if (chosenClassType == foodProducer) {
				chosenCharacter.classComponent.PopulateAbleFoodProducerClasses(civilianClassChoices);
			}
			else if (chosenClassType == basicResourceProducer) {
				chosenCharacter.classComponent.PopulateBasicProducerClasses(civilianClassChoices, npcSettlement.owner.factionType.type);
			}
			else if (chosenClassType == specialCivilian) {
				chosenCharacter.classComponent.PopulateAbleSpecialCivilianClasses(civilianClassChoices);
			}
			Assert.IsTrue(civilianClassChoices.Count > 0, $"{chosenCharacter.name} will set its initial class but it doesn't have any {chosenClassType} able classes.");
			string chosenClass = CollectionUtilities.GetRandomElement(civilianClassChoices);
			chosenCharacter.classComponent.AssignClass(chosenClass, true);
			chosenCharacter.classComponent.OnUpdateCharacterClass();
#if DEBUG_LOG
			summary = $"{summary}\n\tSet class of {chosenCharacter.name} to {chosenClass}";
#endif
			RuinarchListPool<string>.Release(civilianClassChoices);
		}
		RuinarchListPool<Character>.Release(charactersToChangeClass);
#if DEBUG_LOG
		Debug.Log(summary);
#endif
		RuinarchListPool<string>.Release(civilianClassOrder);
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
