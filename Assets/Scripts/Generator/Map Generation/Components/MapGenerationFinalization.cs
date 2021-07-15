using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Events.World_Events;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Locations.Area_Features;
using Managers;
using Pathfinding;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;
using Debug = UnityEngine.Debug;

public class MapGenerationFinalization : MapGenerationComponent {

	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Finalizing World...");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		stopwatch.Stop();
		AddLog($"FinalizeInnerMaps took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");

		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(RegionalItemGeneration());
		stopwatch.Stop();
		AddLog($"RegionalItemGeneration took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
		
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(LandmarkItemGeneration());
		stopwatch.Stop();
		AddLog($"LandmarkItemGeneration took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
		
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(CaveItemGeneration());
		stopwatch.Stop();
		AddLog($"CaveItemGeneration took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
		
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(LoadSettlementItems());
		stopwatch.Stop();
		AddLog($"LoadSettlementItems took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
		
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(CharacterFinalization());
		stopwatch.Stop();
		AddLog($"CharacterFinalization took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");

		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(CreateWorldEvents());
		stopwatch.Stop();
		AddLog($"CreateWorldEvents took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
	}

	#region Events
	private IEnumerator CreateWorldEvents() {
		//WorldEventManager.Instance.AddActiveEvent(new VillagerMigration());
		WorldEventManager.Instance.AddActiveEvent(new CultLeaderEvent());
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			WorldEventManager.Instance.AddActiveEvent(new UndeadAttackEvent());
		}
		yield return null;
	}
	private IEnumerator LoadWorldEvents(SaveDataCurrentProgress saveData) {
		for (int i = 0; i < saveData.worldMapSave.worldEventSaves.Count; i++) {
			SaveDataWorldEvent saveDataWorldEvent = saveData.worldMapSave.worldEventSaves[i];
			WorldEvent worldEvent = saveDataWorldEvent.Load();
			WorldEventManager.Instance.LoadEvent(worldEvent);
		}
		yield return null;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	public static void ItemGenerationAfterPickingLoadout() {
		GenerateArtifacts();
		// if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
		// 	//spawn 1 desert rose
		// 	Region region = GridMap.Instance.allRegions[0];
		// 	LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		// 	List<LocationGridTile> locationChoices = wilderness.unoccupiedTiles.Where(t =>
		// 		t.collectionOwner.isPartOfParentRegionMap && !t.IsAtEdgeOfMap() &&
		// 		t.hexTileOwner.settlementOnTile == null &&
		// 		!t.hexTileOwner.IsAtEdgeOfMap() &&
		// 		t.hexTileOwner.elevationType == ELEVATION.PLAIN).ToList();
		// 	LocationGridTile desertRoseLocation = CollectionUtilities.GetRandomElement(locationChoices);
		// 	desertRoseLocation.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.DESERT_ROSE), desertRoseLocation);
		// 	locationChoices.Remove(desertRoseLocation);
		// 	Debug.Log($"Placed desert rose at {desertRoseLocation.localPlace.ToString()}");	
		// }
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Finalizing World...");
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		yield return MapGenerator.Instance.StartCoroutine(ExecuteLoadedFeatureInitialActions());
		yield return MapGenerator.Instance.StartCoroutine(LoadWorldEvents(saveData));
		yield return null;
#if DEBUG_LOG
		Debug.Log($"Unused pathfinding tags are {InnerMapManager.Instance.unusedPathfindingTags.ComafyList()}");
#endif
	}
	private IEnumerator ExecuteLoadedFeatureInitialActions() {
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
			Area tile = GridMap.Instance.allAreas[i];
			for (int j = 0; j < tile.featureComponent.features.Count; j++) {
				AreaFeature feature = tile.featureComponent.features[j];
				feature.LoadedGameStartActions(tile);
			}
			yield return null;
		}
	}
	#endregion
	
	private IEnumerator FinalizeInnerMaps() {
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			yield return MapGenerator.Instance.StartCoroutine(map.CreateSeamlessEdges());
			foreach (var progress in AstarPath.active.ScanAsync(new NavGraph[] {map.pathfindingGraph, map.unwalkableGraph})) ;
				// PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
			// PathfindingManager.Instance.RescanGrid(map.unwalkableGraph);
			yield return null;
		}
		for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++) {
			VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[i];
			villageSpot.DetermineMigrationSpawningArea();
		}
	}

	#region Item Generation
	private IEnumerator LoadSettlementItems() {
		for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
			NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
			if (npcSettlement.locationType != LOCATION_TYPE.DUNGEON) {
				InnerMapManager.Instance.LoadInitialSettlementItems(npcSettlement);
				yield return null;	
			}
		}
	}
	private IEnumerator RegionalItemGeneration() {
		Region region = GridMap.Instance.mainRegion;
		LocationStructure wilderness = region.wilderness;
		List<LocationGridTile> locationChoices = RuinarchListPool<LocationGridTile>.Claim(); //wilderness.unoccupiedTiles.Where(t => t.area.settlementOnArea == null && t.elevationType == ELEVATION.PLAIN).ToList();
        for (int i = 0; i < wilderness.unoccupiedTiles.Count; i++) {
			LocationGridTile t = wilderness.unoccupiedTiles[i];
            if (t.area.settlementOnArea == null && t.elevationType == ELEVATION.PLAIN) {
				locationChoices.Add(t);
			}
        }
		if (locationChoices.Count > 0) {
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
				//spawn 4 water crystals in other region
				for (int j = 0; j < 4; j++) {
					if (locationChoices.Count == 0) { break; } //no more location choices
					LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
					chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_CRYSTAL), chosenTile);
					locationChoices.Remove(chosenTile);
				}
				//spawn faction heirloom
				LocationStructure heirloomLocation = region.GetRandomStructureOfType(STRUCTURE_TYPE.BARRACKS);
				if (heirloomLocation == null) {
					heirloomLocation = region.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
				}
				LocationGridTile heirloomTile = CollectionUtilities.GetRandomElement(heirloomLocation.unoccupiedTiles);
				TileObject heirloom = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEIRLOOM);
				heirloomTile.structure.AddPOI(heirloom, heirloomTile);
				Faction faction = FactionManager.Instance.GetMajorFactionWithRace(RACE.HUMANS).First();
				faction.SetFactionHeirloom(heirloom);

				RandomRegionalItemGeneration(region, ref locationChoices);
			} else {
				RandomRegionalItemGeneration(region, ref locationChoices);
				if (WorldConfigManager.Instance.isTutorialWorld && locationChoices.Count > 0) {
					//spawn 7 chests randomly
					for (int j = 0; j < 7; j++) {
						if (locationChoices.Count == 0) { break; } //no more location choices
						LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
						chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREASURE_CHEST), chosenTile);
						locationChoices.Remove(chosenTile);
					}
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(locationChoices);
		yield return null;
		//for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
		//	Region region = GridMap.Instance.allRegions[i];
		//	LocationStructure wilderness = region.wilderness;
		//	List<LocationGridTile> locationChoices = wilderness.unoccupiedTiles.Where(t =>
		//		t.area.settlementOnArea == null && t.elevationType == ELEVATION.PLAIN).ToList();
		//	if (locationChoices.Count > 0) {
		//		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
		//			if (i == 1) {
		//				//spawn multiple tombstones
		//				string[] _classChoices = new[] {"Barbarian", "Archer", "Noble", "Farmer"};
		//				int randomAmount = Random.Range(20, 30);
		//				for (int j = 0; j < randomAmount; j++) {
		//					if (locationChoices.Count == 0) { break; } //no more location choices
		//					LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
		//					Tombstone tombstone = InnerMapManager.Instance.CreateNewTileObject<Tombstone>(TILE_OBJECT_TYPE.TOMBSTONE);
		//					locationChoices.Remove(chosenTile);


		//					Character character = CharacterManager.Instance.CreateNewCharacter(CollectionUtilities.GetRandomElement(_classChoices), RACE.ELVES, 
		//						GameUtilities.RollChance(50) ? GENDER.MALE : GENDER.FEMALE, homeRegion: chosenTile.structure.region);
		//                          character.SetIsPreplaced(true);
		//                          character.CreateMarker();
		//					character.InitialCharacterPlacement(chosenTile);
		//					character.marker.UpdatePosition();
		//					character.Death();
		//					tombstone.SetCharacter(character);
		//					chosenTile.structure.AddPOI(tombstone, chosenTile);
		//				}
		//			} else {
		//				//spawn 4 water crystals in other region
		//				for (int j = 0; j < 4; j++) {
		//					if (locationChoices.Count == 0) { break; } //no more location choices
		//					LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
		//					chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_CRYSTAL), chosenTile);
		//					locationChoices.Remove(chosenTile);
		//				}
		//				//spawn faction heirloom
		//				LocationStructure heirloomLocation = region.GetRandomStructureOfType(STRUCTURE_TYPE.BARRACKS);
		//				if (heirloomLocation == null) {
		//					heirloomLocation = region.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		//				}
		//				LocationGridTile heirloomTile = CollectionUtilities.GetRandomElement(heirloomLocation.unoccupiedTiles);
		//				TileObject heirloom = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEIRLOOM);
		//				heirloomTile.structure.AddPOI(heirloom, heirloomTile);
		//				Faction faction = FactionManager.Instance.GetMajorFactionWithRace(RACE.HUMANS).First();
		//				faction.SetFactionHeirloom(heirloom);

		//				RandomRegionalItemGeneration(region, ref locationChoices);
		//			}
		//		} else {
		//			RandomRegionalItemGeneration(region, ref locationChoices);
		//			if (WorldConfigManager.Instance.isTutorialWorld && locationChoices.Count > 0) {
		//				//spawn 7 chests randomly
		//				for (int j = 0; j < 7; j++) {
		//					if (locationChoices.Count == 0) { break; } //no more location choices
		//					LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
		//					chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREASURE_CHEST), chosenTile);
		//					locationChoices.Remove(chosenTile);
		//				}
		//			}
		//		}
		//	}
		//	yield return null;
		//}
	}
	private void RandomRegionalItemGeneration(Region region, ref List<LocationGridTile> locationChoices) {
		ItemGenerationSetting itemGenerationSetting = WorldConfigManager.Instance.worldWideItemGenerationSetting;
		List<ItemSetting> itemChoices = itemGenerationSetting.GetItemChoicesForBiome();
		if (itemChoices != null) {
			ItemSetting randomMonsterSetting = CollectionUtilities.GetRandomElement(itemChoices);
			int randomAmount = Random.Range(1, 5);
			for (int k = 0; k < randomAmount; k++) {
				if (locationChoices.Count == 0) {
					break;
				} //no more location choices
				TILE_OBJECT_TYPE tileObjectType = CollectionUtilities.GetRandomElement(itemChoices).itemType;
				LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
				chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType), chosenTile);
				locationChoices.Remove(chosenTile);
			}
		}
	}
	private IEnumerator LandmarkItemGeneration() {
		List<LocationStructure> allSpecialStructures = RuinarchListPool<LocationStructure>.Claim();
		LandmarkManager.Instance.PopulateAllSpecialStructures(allSpecialStructures);
		for (int i = 0; i < allSpecialStructures.Count; i++) {
			LocationStructure structure = allSpecialStructures[i];
			if (structure.structureType != STRUCTURE_TYPE.CAVE && !(structure is AnimalDen)) {
				StructureData landmarkData = LandmarkManager.Instance.GetStructureData(structure.structureType);
				if (landmarkData.itemGenerationSetting != null) {
					List<ItemSetting> itemChoices = landmarkData.itemGenerationSetting.GetItemChoicesForBiome();
					if (itemChoices != null) {
						int iterations = landmarkData.itemGenerationSetting.iterations.Random();
						for (int j = 0; j < iterations; j++) {
							ItemSetting itemSetting = CollectionUtilities.GetRandomElement(itemChoices);
							int randomAmount = itemSetting.minMaxRange.Random();
							for (int k = 0; k < randomAmount; k++) {
								TILE_OBJECT_TYPE tileObjectType = CollectionUtilities.GetRandomElement(itemChoices).itemType;
								structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType));
							}
						}
						yield return null;
					}
				}
			}
		}
		RuinarchListPool<LocationStructure>.Release(allSpecialStructures);
	}
	private IEnumerator CaveItemGeneration() {
		StructureData caveData = LandmarkManager.Instance.GetStructureData(STRUCTURE_TYPE.CAVE);
		Region region = GridMap.Instance.mainRegion;
		if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
			List<LocationStructure> caves = region.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
			List<ItemSetting> itemChoices = caveData.itemGenerationSetting.GetItemChoicesForBiome();
			for (int j = 0; j < caves.Count; j++) {
				LocationStructure cave = caves[j];
				int hexTileCount = GetHexTileCountOfCave(cave) - 1;
				for (int k = 0; k < hexTileCount; k++) {
					ItemSetting itemSetting = CollectionUtilities.GetRandomElement(itemChoices);
					int randomAmount = itemSetting.minMaxRange.Random();
					for (int l = 0; l < randomAmount; l++) {
						cave.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(itemSetting.itemType));
					}
				}
			}
		}
		yield return null;
		//for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
		//	Region region = GridMap.Instance.allRegions[i];
		//	if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
		//		List<LocationStructure> caves = region.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
		//		List<ItemSetting> itemChoices = caveData.itemGenerationSetting.GetItemChoicesForBiome();
		//		for (int j = 0; j < caves.Count; j++) {
		//			LocationStructure cave = caves[j];
		//			int hexTileCount = GetHexTileCountOfCave(cave) - 1;
		//			for (int k = 0; k < hexTileCount; k++) {
		//				ItemSetting itemSetting = CollectionUtilities.GetRandomElement(itemChoices);
		//				int randomAmount = itemSetting.minMaxRange.Random();
		//				for (int l = 0; l < randomAmount; l++) {
		//					cave.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(itemSetting.itemType));
		//				}
		//			}
		//		}
		//	}
		//	yield return null;
		//}	
	}
	private int GetHexTileCountOfCave(LocationStructure caveStructure) {
		List<Area> tiles = new List<Area>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++) {
			LocationGridTile tile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (tiles.Contains(tile.area) == false) {
				tiles.Add(tile.area);
			}
		}
		return tiles.Count;
	}
	#endregion

	#region Character
	private IEnumerator CharacterFinalization() {
		if (WorldConfigManager.Instance.isTutorialWorld) {
			bool hasEvilCharacter = false;
			bool hasTreacherousCharacter = false;
			List<Character> characterChoices = new List<Character>(CharacterManager.Instance.allCharacters
				.Where(x => x.isNormalCharacter && !x.isDead));
			for (int i = 0; i < characterChoices.Count; i++) {
				Character character = characterChoices[i];
				if (character.traitContainer.HasTrait("Evil")) {
					hasEvilCharacter = true;
				}
				if (character.traitContainer.HasTrait("Treacherous")) {
					hasTreacherousCharacter = true;
				}
			}
		
			//evil character
			if (hasEvilCharacter == false) {
				Character character = CollectionUtilities.GetRandomElement(characterChoices);
				character.traitContainer.AddTrait(character, "Evil");
				characterChoices.Remove(character);
#if DEBUG_LOG
				Debug.Log($"Added evil trait to {character.name}");
#endif
			}
		
			//treacherous
			if (hasTreacherousCharacter == false && characterChoices.Count > 0) {
				Character character = CollectionUtilities.GetRandomElement(characterChoices);
				character.traitContainer.AddTrait(character, "Treacherous");
#if DEBUG_LOG
				Debug.Log($"Added treacherous trait to {character.name}");
#endif
			}	
		}
		
		// List<Character> choices = new List<Character>(CharacterManager.Instance.allCharacters
		// 	.Where(x => x.isNormalCharacter && !x.isDead));
		// for (int i = 0; i < choices.Count; i++) {
		// 	Character character = choices[i];
		// 	// if (GameUtilities.RollChance(30)) {
		// 		TileObject phylactery = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.PHYLACTERY);
		// 		character.ObtainItem(phylactery);
		// 	// }
		// }
		
		yield return null;
	}
#endregion

#region Artifacts
	private static void GenerateArtifacts() {
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			//if demo build, always spawn necronomicon at ancient ruins
			//Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			Region randomRegion = GridMap.Instance.mainRegion;
			//tutorial should always have 2 ancient graveyards.
			LocationStructure ancientRuin = randomRegion.structures[STRUCTURE_TYPE.ANCIENT_GRAVEYARD][1];
			Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Necronomicon);
			ancientRuin.AddPOI(artifact);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			//always spawn Ankh of anubis
			//Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			Region randomRegion = GridMap.Instance.mainRegion;
			LocationStructure targetStructure = randomRegion.wilderness;
			Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Ankh_Of_Anubis);
			targetStructure.AddPOI(artifact);
			// //excalibur
			// randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			// TileObject excalibur = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EXCALIBUR); 
			// randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddPOI(excalibur);
			// Debug.Log($"Placed Excalibur at {excalibur.gridTileLocation}");
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			//Commented this out because of task:
			//https://trello.com/c/YvfQuJ4g/4295-remove-necronomicon-from-pangat-loo
			// //always spawn Necronomicon
			// Region randomRegion = GridMap.Instance.allRegions[0];
			// //tutorial should always have 2 ancient graveyards.
			// LocationStructure structure = randomRegion.structures[STRUCTURE_TYPE.ANCIENT_GRAVEYARD][1];
			// Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Necronomicon);
			// structure.AddPOI(artifact);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			List<LocationStructure> structures = LandmarkManager.Instance.GetStructuresOfType(STRUCTURE_TYPE.TEMPLE);
			if (structures != null) {
				List<ARTIFACT_TYPE> artifactChoices = new List<ARTIFACT_TYPE>() {
					ARTIFACT_TYPE.Necronomicon, ARTIFACT_TYPE.Heart_Of_The_Wind, ARTIFACT_TYPE.Gorgon_Eye, ARTIFACT_TYPE.Berserk_Orb, ARTIFACT_TYPE.Ankh_Of_Anubis
				};
				for (int i = 0; i < structures.Count; i++) {
					if (artifactChoices.Count == 0) { break; }
					LocationStructure structure = structures[i];
					ARTIFACT_TYPE randomArtifact = CollectionUtilities.GetRandomElement(artifactChoices);
					Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(randomArtifact);
					structure.AddPOI(artifact);
					artifactChoices.Remove(randomArtifact);
				}
			}
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			//excalibur
			//Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			Region randomRegion = GridMap.Instance.mainRegion;
			TileObject excalibur = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EXCALIBUR); 
			randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.ANCIENT_RUIN).AddPOI(excalibur);
#if DEBUG_LOG
			Debug.Log($"Placed Excalibur at {excalibur.gridTileLocation}");
#endif
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			List<LocationStructure> landmarks = LandmarkManager.Instance.GetStructuresOfType(STRUCTURE_TYPE.MONSTER_LAIR);
            if (landmarks != null && landmarks.Count > 0) {
				LocationStructure structure = landmarks[0];
				Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Berserk_Orb);
				structure.AddPOI(artifact);
			}
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
			int artifactCount = 4;
			List<ARTIFACT_TYPE> artifactChoices = new List<ARTIFACT_TYPE>() {
				ARTIFACT_TYPE.Necronomicon, ARTIFACT_TYPE.Heart_Of_The_Wind, ARTIFACT_TYPE.Gorgon_Eye, ARTIFACT_TYPE.Berserk_Orb, ARTIFACT_TYPE.Ankh_Of_Anubis
			};
			//randomly generate Artifacts
			for (int i = 0; i < artifactCount; i++) {
				if (artifactChoices.Count == 0) { break; }
				//Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
				Region randomRegion = GridMap.Instance.mainRegion;
				LocationStructure specialStructure = randomRegion.GetRandomStructureThatIsInADungeonAndHasPassableTiles();
				if (specialStructure != null) {
					ARTIFACT_TYPE randomArtifact = CollectionUtilities.GetRandomElement(artifactChoices);
					Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(randomArtifact);
					specialStructure.AddPOI(artifact);
					artifactChoices.Remove(randomArtifact);	
				}
			}
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
			List<LocationStructure> landmarks = LandmarkManager.Instance.GetStructuresOfType(STRUCTURE_TYPE.MAGE_TOWER);
			if (landmarks != null && landmarks.Count > 0) {
				LocationStructure structure = landmarks[0];
				Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Berserk_Orb);
				structure.AddPOI(artifact);
			}
		} else {
			//int artifactCount = GridMap.Instance.allRegions.Length <= 2 ? 1 : 2
			int artifactCount = 1;
			List<TILE_OBJECT_TYPE> artifactChoices = new List<TILE_OBJECT_TYPE>(WorldConfigManager.Instance.initialArtifactChoices);
			//randomly generate Artifacts
			for (int i = 0; i < artifactCount; i++) {
				if (artifactChoices.Count == 0) { break; }
				//Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
				Region randomRegion = GridMap.Instance.mainRegion;
				LocationStructure specialStructure = randomRegion.GetRandomStructureThatIsInADungeonAndHasPassableTiles();
				if (specialStructure != null) {
					TILE_OBJECT_TYPE randomArtifact = CollectionUtilities.GetRandomElement(artifactChoices);
					if (randomArtifact.IsArtifact(out ARTIFACT_TYPE artifactType)) {
						Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(artifactType);
						specialStructure.AddPOI(artifact);
						artifactChoices.Remove(randomArtifact);	
					} else {
						TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomArtifact);
						specialStructure.AddPOI(tileObject);
						artifactChoices.Remove(randomArtifact);	
					}
					
				}
			}
		}
	}
#endregion
}
