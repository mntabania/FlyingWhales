using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Tilemaps;
using UtilityScripts;

/// <summary>
/// Class used to store all data for map generation, this data will be passed around between
/// map generation components.
/// </summary>
public class MapGenerationData {

	//batching values
	public static int WorldMapTileGenerationBatches = 200;
	public static int WorldMapOuterGridGenerationBatches = 200;
	public static int WorldMapElevationRefinementBatches = 400;
	public static int WorldMapFeatureGenerationBatches = 200;
	public static int WorldMapHabitabilityGenerationBatches = 300;
	public static int InnerMapTileGenerationBatches = 500;
	public static int InnerMapSeamlessEdgeBatches = 300;
	public static int InnerMapDetailBatches = 200;
	public static int InnerMapElevationBatches = 300;
	public static int TileObjectLoadingBatches = 300;
	public static int JobLoadingBatches = 300;  
	public static int LocationGridTileSecondaryWaveBatches = 300;
	public static int TileObjectCreationBatches = 10;
	public static int TileObjectCreationBatchesAfterWorldGeneration = 5;
	public static int TileObjectCreationBatchesWhileWaiting = 30;
	
	//constants
	public const float XOffset = 2.56f;
	public const float YOffset = 1.93f;
	
	//world map
	public WorldMapTemplate chosenWorldMapTemplate;
	 
	public List<VillageSpot> villageSpots { get; private set; }
	public List<Area> unreservedAreas { get; private set; }
	public List<Area> reservedAreas { get; private set; }
	public Dictionary<FactionTemplate, List<VillageSpot>> determinedVillages { get; private set; }
	public Dictionary<Area, List<LocationGridTile>> oceanBorderTilesCategorizedByArea { get; private set; }
	public Dictionary<Area, List<LocationGridTile>> caveBorderTilesCategorizedByArea { get; private set; }
	public TILE_OBJECT_TYPE[][] generatedMapPerlinDetailsMap { get; private set; }
	public bool isGeneratingTileObjects { get; private set; }
	public bool hasFinishedMapGenerationCoroutine { get; private set; }
	public List<StructureSetting> unplacedStructuresOnLastEnsuredStructurePlacementCall { get; private set; }
	public List<STRUCTURE_TYPE> LastPlacedStructureTypes { get; private set; }
	public Dictionary<NPCSettlement, int> missingFoodProducers { get; private set; }
	public Dictionary<NPCSettlement, int> missingBasicResourceProducers { get; private set; }

	#region getters
	public int width => chosenWorldMapTemplate.worldMapWidth;
	public int height => chosenWorldMapTemplate.worldMapHeight;
	public int regionCount => chosenWorldMapTemplate.regionCount;
	#endregion

	public MapGenerationData() {
		villageSpots = new List<VillageSpot>();
		determinedVillages = new Dictionary<FactionTemplate, List<VillageSpot>>();
		oceanBorderTilesCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
		caveBorderTilesCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
		missingFoodProducers = new Dictionary<NPCSettlement, int>();
		missingBasicResourceProducers = new Dictionary<NPCSettlement, int>();
		LastPlacedStructureTypes = new List<STRUCTURE_TYPE>();
	}

	#region General
	public void SetFinishedMapGenerationCoroutine(bool p_state) {
		hasFinishedMapGenerationCoroutine = p_state;
	}
	#endregion
	
	#region Village Spots
	public VillageSpot AddVillageSpot(Area p_villageSpot, List<Area> p_areas, int p_lumberyardSpots, int p_miningSpots) {
		VillageSpot villageSpot = new VillageSpot(p_villageSpot, p_areas, p_lumberyardSpots, p_miningSpots);
		villageSpots.Add(villageSpot);
		return villageSpot;
	}
	public void RemoveVillageSpot(VillageSpot p_villageSpot) {
		villageSpots.Remove(p_villageSpot);
	}
	public void AddDeterminedVillage(FactionTemplate p_faction, VillageSpot p_spot) {
		if (!determinedVillages.ContainsKey(p_faction)) {
			determinedVillages.Add(p_faction, new List<VillageSpot>());
		}
		determinedVillages[p_faction].Add(p_spot);
#if DEBUG_LOG
		Debug.Log($"Determined Village for {p_faction.name}: {p_spot.ToString()}");
#endif
	}
	public void SetUnreservedAreas(List<Area> p_areas) {
		unreservedAreas = p_areas;
	}
	public void SetReservedAreas(List<Area> p_areas) {
		reservedAreas = p_areas;
	}
	#endregion

	#region Border Tiles
	public void AddOceanBorderTile(Area p_area, LocationGridTile p_tile) {
		if (!oceanBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			oceanBorderTilesCategorizedByArea.Add(p_area, RuinarchListPool<LocationGridTile>.Claim());
		}
		oceanBorderTilesCategorizedByArea[p_area].Add(p_tile);
	}
	public void RemoveOceanBorderTile(Area p_area, LocationGridTile p_tile) {
		if (oceanBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			oceanBorderTilesCategorizedByArea[p_area].Remove(p_tile);
			if (oceanBorderTilesCategorizedByArea[p_area].Count == 0) {
				oceanBorderTilesCategorizedByArea.Remove(p_area);
			}
		}
	}
	public LocationGridTile GetFirstUnoccupiedNonEdgeOceanTile(Area p_area) {
		if (oceanBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			List<LocationGridTile> tiles = oceanBorderTilesCategorizedByArea[p_area];
			for (int i = 0; i < tiles.Count; i++) {
				LocationGridTile tile = tiles[i];
				if (tile.tileObjectComponent.objHere == null && !tile.IsAtEdgeOfMap() && tile.HasDifferentStructureNeighbour(true)) {
					return tile;
				}
			}
		}
		return null;
	}
	public void AddCaveBorderTile(Area p_area, LocationGridTile p_tile) {
		if (!caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			caveBorderTilesCategorizedByArea.Add(p_area, RuinarchListPool<LocationGridTile>.Claim());
		}
		caveBorderTilesCategorizedByArea[p_area].Add(p_tile);
	}
	public void RemoveCaveBorderTile(Area p_area, LocationGridTile p_tile) {
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			caveBorderTilesCategorizedByArea[p_area].Remove(p_tile);
			if (caveBorderTilesCategorizedByArea[p_area].Count == 0) {
				caveBorderTilesCategorizedByArea.Remove(p_area);
			}
		}
	}
	public LocationGridTile GetFirstUnoccupiedNonEdgeCaveTile(Area p_area, MapGenerationData p_data) {
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			List<LocationGridTile> tiles = caveBorderTilesCategorizedByArea[p_area];
			for (int i = 0; i < tiles.Count; i++) {
				LocationGridTile tile = tiles[i];
				if (IsTileValidOreVeinTarget(tile, p_data)) {
					return tile;
				}
			}
		}
		return null;
	}
	public LocationGridTile GetFirstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot(Area p_area, MapGenerationData p_data, Area p_villageSpot) {
#if DEBUG_LOG
		string summary = $"Generating ore vein at {p_area} for village spot at {p_villageSpot}. ";
#endif
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			List<LocationGridTile> caveBorderTiles = caveBorderTilesCategorizedByArea[p_area];
#if DEBUG_LOG
			summary = $"{summary}Will determine tiles facing village spot. Cave border tiles are:\n{caveBorderTiles.ComafyList()}";
#endif
			List<LocationGridTile> tilesFacingArea = RuinarchListPool<LocationGridTile>.Claim();
			PopulateTilesFacingArea(tilesFacingArea, caveBorderTiles, p_villageSpot);
#if DEBUG_LOG
			summary = $"{summary}\nTiles facing village spot are:\n{tilesFacingArea.ComafyList()}";
#endif
			for (int i = 0; i < tilesFacingArea.Count; i++) {
				LocationGridTile tileFacingArea = tilesFacingArea[i];
				
				bool isTooNearToVillageSpotArea = false;
				List<LocationGridTile> neighbours = RuinarchListPool<LocationGridTile>.Claim();
				tileFacingArea.PopulateTilesInRadius(neighbours, 1, includeTilesInDifferentStructure: true);
				for (int j = 0; j < neighbours.Count; j++) {
					LocationGridTile neighbour = neighbours[j];
					if (neighbour.area == p_villageSpot) {
						isTooNearToVillageSpotArea = true;
						break;
					}
				}
				RuinarchListPool<LocationGridTile>.Release(neighbours);
				bool isTileValidOreVeinTarget = IsTileValidOreVeinTarget(tileFacingArea, p_data);
#if DEBUG_LOG
				summary = $"{summary}\n\t-Evaluating {tileFacingArea}. is too near to village: {isTooNearToVillageSpotArea}. Is valid ore vein target {isTileValidOreVeinTarget.ToString()}";
#endif
				if (!isTooNearToVillageSpotArea && isTileValidOreVeinTarget) {
					RuinarchListPool<LocationGridTile>.Release(tilesFacingArea);
#if DEBUG_LOG
					summary = $"{summary}\n\t-Found valid tile {tileFacingArea}!";
					Debug.Log(summary);
#endif
					return tileFacingArea;
				}
			}
			//
			// for (int i = 0; i < caveBorderTiles.Count; i++) {
			// 	LocationGridTile tile = caveBorderTiles[i];
			// 	if (IsTileValidOreVeinTarget(tile, p_data)) {
			// 		return tile;
			// 	}
			// }
			RuinarchListPool<LocationGridTile>.Release(tilesFacingArea);

		} else {
#if DEBUG_LOG
			summary = $"{summary}\nArea doesn't have key in caveBorderTilesCategorizedByArea";
#endif
		}
#if DEBUG_LOG
		Debug.Log(summary);
#endif
		return null;
	}

	private bool IsTileValidOreVeinTarget(LocationGridTile p_tile, MapGenerationData p_data) {
		if ((p_tile.tileObjectComponent.objHere is BlockWall || p_data.GetGeneratedObjectOnTile(p_tile) == TILE_OBJECT_TYPE.BLOCK_WALL) && !p_tile.IsAtEdgeOfMap()) {
			if (p_tile.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.ORE_VEIN) > 0) {
				return false;
			}
			List<LocationGridTile> fourNeighbours = p_tile.FourNeighbours();
			int wildernessNeighboursInCardinal = fourNeighbours.Count(t => t.structure is Wilderness);
			if (wildernessNeighboursInCardinal == 1 || wildernessNeighboursInCardinal == 2) {
				List<LocationGridTile> allNeighbours = p_tile.neighbourList;
				int wildernessNeighboursInOrdinal = allNeighbours.Count(t => t.structure is Wilderness);
				if (wildernessNeighboursInOrdinal == 3 || wildernessNeighboursInOrdinal == 2) {
					return true;
				}
			}
		}
		return false;
	}
	private int AllowedHeightTolerance = 5;
	private void PopulateTilesFacingArea(List<LocationGridTile> p_listToPopulate, List<LocationGridTile> p_choices, Area p_area) {
		LocationGridTile centerTileOfArea = p_area.gridTileComponent.centerGridTile;
		for (int i = 0; i < p_choices.Count; i++) {
			LocationGridTile tile = p_choices[i];
			GridNeighbourDirection directionFromTileToArea = GridNeighbourDirection.North;
			if (tile.localPlace.x < centerTileOfArea.localPlace.x) {
				//east
				int yDifference = Mathf.Abs(tile.localPlace.y - centerTileOfArea.localPlace.y); 
				if (tile.localPlace.y < centerTileOfArea.localPlace.y && yDifference > AllowedHeightTolerance) {
					directionFromTileToArea = GridNeighbourDirection.North_East;	
				} else if (tile.localPlace.y > centerTileOfArea.localPlace.y && yDifference > AllowedHeightTolerance) {
					directionFromTileToArea = GridNeighbourDirection.South_East;	
				} else {
					directionFromTileToArea = GridNeighbourDirection.East;	
				}
			} else if (tile.localPlace.x > centerTileOfArea.localPlace.x) {
				//west
				int yDifference = Mathf.Abs(tile.localPlace.y - centerTileOfArea.localPlace.y);
				if (tile.localPlace.y < centerTileOfArea.localPlace.y && yDifference > AllowedHeightTolerance) {
					directionFromTileToArea = GridNeighbourDirection.North_West;	
				} else if (tile.localPlace.y > centerTileOfArea.localPlace.y && yDifference > AllowedHeightTolerance) {
					directionFromTileToArea = GridNeighbourDirection.South_West;	
				} else {
					directionFromTileToArea = GridNeighbourDirection.West;	
				}
			} else if (tile.localPlace.x == centerTileOfArea.localPlace.x) {
				//same place as center of village spot. check difference in y position
				if (tile.localPlace.y < centerTileOfArea.localPlace.y) {
					directionFromTileToArea = GridNeighbourDirection.North;	
				} else if (tile.localPlace.y > centerTileOfArea.localPlace.y) {
					directionFromTileToArea = GridNeighbourDirection.South;	
				} else {
					//same x and y as center tile of given area, logic should NEVER reach this point, but handled it just in case.
					directionFromTileToArea = GridNeighbourDirection.North;	
				}
			}
			// else if (tile.localPlace.y < centerTileOfArea.localPlace.y) {
			// 	directionFromTileToArea = GridNeighbourDirection.North;
			// } else if (tile.localPlace.y > centerTileOfArea.localPlace.y) {
			// 	directionFromTileToArea = GridNeighbourDirection.South;
			// }
			LocationGridTile neighbourInDirectionOfArea = tile.GetNeighbourAtDirection(directionFromTileToArea);
			if (neighbourInDirectionOfArea != null && neighbourInDirectionOfArea.structure.structureType == STRUCTURE_TYPE.WILDERNESS) {
				p_listToPopulate.Add(tile);
			}
		}
	}
	#endregion

	#region Tile Objects
	public void InitializeGeneratedMapPerlinDetails(int width, int height) {
		TILE_OBJECT_TYPE[][] generatedObjectMap = new TILE_OBJECT_TYPE[width][];
		for (int x = 0; x < width; x++) {
			generatedObjectMap[x] = new TILE_OBJECT_TYPE[height];
			for (int y = 0; y < height; y++) {
				generatedObjectMap[x][y] = TILE_OBJECT_TYPE.NONE;
			}
		}
		generatedMapPerlinDetailsMap = generatedObjectMap;
	}
	public void SetGeneratedMapPerlinDetails(LocationGridTile tileLocation, TILE_OBJECT_TYPE p_type) {
		generatedMapPerlinDetailsMap[tileLocation.localPlace.x][tileLocation.localPlace.y] = p_type;
		if (p_type == TILE_OBJECT_TYPE.NONE) {
			tileLocation.parentMap.detailsTilemap.SetTile(tileLocation.localPlace, null);
		} else {
			TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject(p_type);
			TileBase tileBaseToUse = tileObjectScriptableObject.GetTileBaseToUse(tileLocation.mainBiomeType);
			tileLocation.parentMap.detailsTilemap.SetTile(tileLocation.localPlace, tileBaseToUse);
		}
		// Debug.Log($"Set Generated Map Perlin detail on {tileLocation} to {p_type.ToString()}");
	}
	public void SetGeneratingTileObjectsState(bool p_state) {
		isGeneratingTileObjects = p_state;
	}
	public TILE_OBJECT_TYPE GetGeneratedObjectOnTile(LocationGridTile p_tile) {
		return generatedMapPerlinDetailsMap[p_tile.localPlace.x][p_tile.localPlace.y];
	}
	#endregion

	#region Helpers
	public void AddLastPlacedStructureTypes(STRUCTURE_TYPE p_types) {
		LastPlacedStructureTypes.Add(p_types);
	}
	public void ClearLastPlacedVillageStructures() {
		LastPlacedStructureTypes.Clear();
	}
	public void SetLastUnplacedStructures(List<StructureSetting> p_settings) {
		unplacedStructuresOnLastEnsuredStructurePlacementCall = p_settings;
	}

	public void SetMissingFoodProducers(NPCSettlement p_settlement, int p_count) {
		if (!missingFoodProducers.ContainsKey(p_settlement)) {
			missingFoodProducers.Add(p_settlement, 0);
		}
		missingFoodProducers[p_settlement] = p_count;
	}
	public void SetMissingBasicResourceProducers(NPCSettlement p_settlement, int p_count) {
		if (!missingBasicResourceProducers.ContainsKey(p_settlement)) {
			missingBasicResourceProducers.Add(p_settlement, 0);
		}
		missingBasicResourceProducers[p_settlement] = p_count;
	}
	public int GetTotalMissingProductionStructures(NPCSettlement p_settlement) {
		int missing = 0;
		if (missingFoodProducers.ContainsKey(p_settlement)) {
			missing += missingFoodProducers[p_settlement];
		}
		if (missingBasicResourceProducers.ContainsKey(p_settlement)) {
			missing += missingBasicResourceProducers[p_settlement];
		}

		return missing;
	}
	#endregion

	#region Clean Up
	public void CleanUpAfterMapGeneration() {
		foreach (var kvp in oceanBorderTilesCategorizedByArea) {
			RuinarchListPool<LocationGridTile>.Release(kvp.Value);
		}
		foreach (var kvp in caveBorderTilesCategorizedByArea) {
			RuinarchListPool<LocationGridTile>.Release(kvp.Value);
		}
		oceanBorderTilesCategorizedByArea.Clear();
		caveBorderTilesCategorizedByArea.Clear();
		oceanBorderTilesCategorizedByArea = null;
		caveBorderTilesCategorizedByArea = null;
		generatedMapPerlinDetailsMap = null;
		unplacedStructuresOnLastEnsuredStructurePlacementCall = null;
		LastPlacedStructureTypes = null;
		missingFoodProducers = null;
		missingBasicResourceProducers = null;
	}
	#endregion
}