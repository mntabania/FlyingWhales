using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class MapGenerationFinalization : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions());
		yield return MapGenerator.Instance.StartCoroutine(RegionalItemGeneration());
		yield return MapGenerator.Instance.StartCoroutine(LandmarkItemGeneration());
		yield return MapGenerator.Instance.StartCoroutine(CaveItemGeneration());
		yield return MapGenerator.Instance.StartCoroutine(LoadItems());
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i]; 
			region.GenerateOuterBorders();
			region.HideBorders();
		}
		data.familyTreeDatabase.Save();
	}

	private IEnumerator FinalizeInnerMaps() {
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			yield return MapGenerator.Instance.StartCoroutine(map.CreateSeamlessEdges());
			PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
			yield return null;
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
		
		// TILE_OBJECT_TYPE[] crystalChoices = new[] {
		// 	TILE_OBJECT_TYPE.FIRE_CRYSTAL, TILE_OBJECT_TYPE.ICE_CRYSTAL, TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL,
		// 	TILE_OBJECT_TYPE.POISON_CRYSTAL, TILE_OBJECT_TYPE.WATER_CRYSTAL
		// };
		// for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
		// 	Region region = GridMap.Instance.allRegions[i];
		// 	LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		// 	for (int j = 0; j < 3; j++) {
		// 		wilderness.AddPOI(
		// 			InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MIMIC_TILE_OBJECT));	
		// 	}
		// 	
		// 	for (int j = 0; j < 30; j++) {
		// 		TILE_OBJECT_TYPE crystalType = CollectionUtilities.GetRandomElement(crystalChoices);
		// 		wilderness.AddPOI(
		// 			InnerMapManager.Instance.CreateNewTileObject<TileObject>(crystalType));
		// 	}
		// }
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
	
	
}
