using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Locations.Tile_Features;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;

public class MapGenerationFinalization : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Finalizing world...");
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions());
		//yield return MapGenerator.Instance.StartCoroutine(RegionalItemGeneration());
		//yield return MapGenerator.Instance.StartCoroutine(LandmarkItemGeneration());
		//yield return MapGenerator.Instance.StartCoroutine(CaveItemGeneration());
		//yield return MapGenerator.Instance.StartCoroutine(LoadItems());
		//yield return MapGenerator.Instance.StartCoroutine(CharacterFinalization());
		//yield return MapGenerator.Instance.StartCoroutine(LoadArtifacts());
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i]; 
			region.GenerateOuterBorders();
			region.HideBorders();
		}
		//data.familyTreeDatabase.Save();
	}

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		//TODO:
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	private IEnumerator FinalizeInnerMaps() {
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			yield return MapGenerator.Instance.StartCoroutine(map.CreateSeamlessEdges());
			PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
			yield return null;
			map.PredetermineGraphNodes();
		}
	}

	private IEnumerator ExecuteFeatureInitialActions() {
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile tile = GridMap.Instance.normalHexTiles[i];
			for (int j = 0; j < tile.featureComponent.features.Count; j++) {
				TileFeature feature = tile.featureComponent.features[j];
				feature.GameStartActions(tile);
			}
			yield return null;
		}
	}

	#region Item Generation
	private IEnumerator LoadItems() {
		for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
			NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
			if (npcSettlement.locationType != LOCATION_TYPE.DUNGEON) {
				InnerMapManager.Instance.LoadInitialSettlementItems(npcSettlement);
				yield return null;	
			}
		}

		if (!WorldConfigManager.Instance.isTutorialWorld) {
			Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			TileObject excalibur = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EXCALIBUR); 
			randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddPOI(excalibur);
			Debug.Log($"Placed Excalibur at {excalibur.gridTileLocation}");	
		}
	}
	private IEnumerator RegionalItemGeneration() {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
			List<LocationGridTile> locationChoices = wilderness.unoccupiedTiles.Where(t =>
				t.collectionOwner.isPartOfParentRegionMap &&
				t.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == null &&
				t.collectionOwner.partOfHextile.hexTileOwner.elevationType == ELEVATION.PLAIN).ToList();
			if (locationChoices.Count > 0) {
				ItemGenerationSetting itemGenerationSetting =
					WorldConfigManager.Instance.worldWideItemGenerationSetting;
				List<ItemSetting> itemChoices = itemGenerationSetting.GetItemChoicesForBiome(region.coreTile.biomeType);
				if (itemChoices != null) {
					int iterations = itemGenerationSetting.iterations.Random();
					for (int j = 0; j < iterations; j++) {
						if (locationChoices.Count == 0) { break; } //no more location choices
						ItemSetting randomMonsterSetting = CollectionUtilities.GetRandomElement(itemChoices);
						int randomAmount = randomMonsterSetting.minMaxRange.Random();
						for (int k = 0; k < randomAmount; k++) {
							if (locationChoices.Count == 0) { break; } //no more location choices
							TILE_OBJECT_TYPE tileObjectType = CollectionUtilities.GetRandomElement(itemChoices).itemType;
							LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
							chosenTile.structure.AddPOI(
								InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType), chosenTile);
							locationChoices.Remove(chosenTile);
						}
					}	
				}
				if (WorldConfigManager.Instance.isTutorialWorld && locationChoices.Count > 0) {
					//spawn 7 chests randomly
					for (int j = 0; j < 7; j++) {
						if (locationChoices.Count == 0) { break; } //no more location choices
						LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(locationChoices);
						chosenTile.structure.AddPOI(
							InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREASURE_CHEST), chosenTile);
						locationChoices.Remove(chosenTile);
					}
				}
			}
			yield return null;
		}
	}
	private IEnumerator LandmarkItemGeneration() {
		List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
		for (int i = 0; i < allLandmarks.Count; i++) {
			BaseLandmark landmark = allLandmarks[i];
			if (landmark.specificLandmarkType != LANDMARK_TYPE.CAVE) {
				LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
				LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmark.specificLandmarkType);
				if (landmarkData.itemGenerationSetting != null) {
					List<ItemSetting> itemChoices = landmarkData.itemGenerationSetting.
						GetItemChoicesForBiome(landmark.tileLocation.biomeType);
					if (itemChoices != null) {
						int iterations = landmarkData.itemGenerationSetting.iterations.Random();
						for (int j = 0; j < iterations; j++) {
							ItemSetting itemSetting = CollectionUtilities.GetRandomElement(itemChoices);
							int randomAmount = itemSetting.minMaxRange.Random();
							for (int k = 0; k < randomAmount; k++) {
								TILE_OBJECT_TYPE tileObjectType = CollectionUtilities.GetRandomElement(itemChoices).itemType;
								structure.AddPOI(
									InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType));
							}
						}
						yield return null;
					}
				}
			}
		}
	}
	private IEnumerator CaveItemGeneration() {
		LandmarkData caveData = LandmarkManager.Instance.GetLandmarkData(LANDMARK_TYPE.CAVE);
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
				List<LocationStructure> caves = region.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.CAVE);
				List<ItemSetting> itemChoices = caveData.itemGenerationSetting.GetItemChoicesForBiome(region.coreTile.biomeType);
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
		}	
	}
	private int GetHexTileCountOfCave(LocationStructure caveStructure) {
		List<HexTile> tiles = new List<HexTile>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++) {
			LocationGridTile tile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (tile.collectionOwner.isPartOfParentRegionMap && tiles.Contains(tile.collectionOwner.partOfHextile.hexTileOwner) == false) {
				tiles.Add(tile.collectionOwner.partOfHextile.hexTileOwner);
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
				Debug.Log($"Added evil trait to {character.name}");
			}
		
			//treacherous
			if (hasTreacherousCharacter == false && characterChoices.Count > 0) {
				Character character = CollectionUtilities.GetRandomElement(characterChoices);
				character.traitContainer.AddTrait(character, "Treacherous");
				Debug.Log($"Added treacherous trait to {character.name}");
			}	
		}
		
		yield return null;
	}
	#endregion

	#region Artifacts
	private IEnumerator LoadArtifacts() {
		List<ARTIFACT_TYPE> artifactChoices = WorldConfigManager.Instance.initialArtifactChoices;

		if (WorldConfigManager.Instance.isTutorialWorld) {
			//if demo build, always spawn necronomicon at ancient ruins
			artifactChoices.Remove(ARTIFACT_TYPE.Necronomicon);
			Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			//tutorial should always have 2 ancient graveyards.
			LocationStructure ancientRuin = randomRegion.structures[STRUCTURE_TYPE.ANCIENT_GRAVEYARD][1];
			Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Necronomicon);
			ancientRuin.AddPOI(artifact);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			//if demo build, always spawn Ankh of anubis
			artifactChoices.Remove(ARTIFACT_TYPE.Ankh_Of_Anubis);
			Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			LocationStructure targetStructure = randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
			Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Ankh_Of_Anubis);
			targetStructure.AddPOI(artifact);
		} else {
			//randomly generate 3 Artifacts
			for (int i = 0; i < 3; i++) {
				if (artifactChoices.Count == 0) { break; }
				Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
				LocationStructure wilderness = randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
				ARTIFACT_TYPE randomArtifact = CollectionUtilities.GetRandomElement(artifactChoices);
				Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(randomArtifact);
				wilderness.AddPOI(artifact);
				artifactChoices.Remove(randomArtifact);
			}
		}
		yield return null;
	}
	#endregion
	
}
