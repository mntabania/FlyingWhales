using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using UtilityScripts;
using Random = System.Random;

public class ElevationStructureGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		// for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
		// 	Region region = GridMap.Instance.allRegions[i];
		// 	List<ElevationIsland> islandsInRegion = GetElevationIslandsInRegion(region);
		// 	for (int j = 0; j < islandsInRegion.Count; j++) {
		// 		ElevationIsland currIsland = islandsInRegion[j];
		// 		STRUCTURE_TYPE structureType = GetStructureTypeFor(currIsland.elevation);
		// 		NPCSettlement settlement = null;
		// 		if (structureType == STRUCTURE_TYPE.CAVE) {
		// 			//only create settlement for caves
		// 			settlement = LandmarkManager.Instance.CreateNewSettlement(region, LOCATION_TYPE.DUNGEON, currIsland.tilesInIsland.ToArray());	
		// 		}
		// 		LocationStructure elevationStructure = LandmarkManager.Instance.CreateNewStructureAt(region, structureType, settlement);
		// 		
		// 		yield return MapGenerator.Instance.StartCoroutine(GenerateElevationMap(currIsland, elevationStructure));
		// 		yield return MapGenerator.Instance.StartCoroutine(RefreshTilemapCollider(region.innerMap.structureTilemapCollider));
		// 	}
		// }
		// GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.SetActive(true);
		// yield return MapGenerator.Instance.StartCoroutine(GridMap.Instance.mainRegion.innerMap.DrawElevationIslands(data.elevationIslands));
		yield return null;
	}
	// private IEnumerator RefreshTilemapCollider(TilemapCollider2D tilemapCollider2D) {
	// 	tilemapCollider2D.enabled = false;
	// 	yield return new WaitForSeconds(0.5f);
	// 	// ReSharper disable once Unity.InefficientPropertyAccess
	// 	tilemapCollider2D.enabled = true;
	// }
	// private STRUCTURE_TYPE GetStructureTypeFor(ELEVATION elevation) {
	// 	switch (elevation) {
	// 		case ELEVATION.MOUNTAIN:
	// 			return STRUCTURE_TYPE.CAVE;
	// 		case ELEVATION.WATER:
	// 			return STRUCTURE_TYPE.OCEAN;
	// 	}
	// 	throw new Exception($"There is no corresponding structure type for {elevation.ToString()}");
	// }
	// private List<AreaElevationIsland> GetElevationIslandsInRegion(Region region) {
	// 	List<AreaElevationIsland> islands = new List<AreaElevationIsland>();
	// 	ELEVATION[] elevationsToCheck = new[] {ELEVATION.WATER, ELEVATION.MOUNTAIN};
	// 	for (int i = 0; i < elevationsToCheck.Length; i++) {
	// 		ELEVATION elevation = elevationsToCheck[i];
	// 		List<Area> tilesOfThatElevation = GetTilesWithElevationInRegion(region, elevation);
	// 		List<AreaElevationIsland> initialIslands = CreateInitialIslands(tilesOfThatElevation, elevation);
	// 		List<AreaElevationIsland> mergedIslands = MergeIslands(initialIslands);
	// 		islands.AddRange(mergedIslands);
	// 	}
	// 	return islands;
	// }
	// private List<Area> GetTilesWithElevationInRegion(Region region, ELEVATION elevation) {
	// 	List<Area> tiles = new List<Area>();
	// 	for (int i = 0; i < region.areas.Count; i++) {
	// 		Area tile = region.areas[i];
	// 		if (tile.elevationType == elevation) {
	// 			tiles.Add(tile);
	// 		}
	// 	}
	// 	return tiles;
	// }
	// private List<AreaElevationIsland> CreateInitialIslands(List<Area> tiles, ELEVATION elevation) {
	// 	List<AreaElevationIsland> islands = new List<AreaElevationIsland>();
	// 	for (int i = 0; i < tiles.Count; i++) {
	// 		Area tile = tiles[i];
	// 		AreaElevationIsland island = new AreaElevationIsland(elevation);
	// 		island.AddTile(tile);
	// 		islands.Add(island);
	// 	}
	// 	return islands;
	// }
	// private List<AreaElevationIsland> MergeIslands(List<AreaElevationIsland> islands) {
	// 	for (int i = 0; i < islands.Count; i++) {
	// 		AreaElevationIsland currIsland = islands[i];
	// 		for (int j = 0; j < islands.Count; j++) {
	// 			AreaElevationIsland otherIsland = islands[j];
	// 			if (currIsland != otherIsland) {
	// 				if (currIsland.IsAdjacentToIsland(otherIsland)) {
	// 					currIsland.MergeWithIsland(otherIsland);
	// 				}
	// 			}
	// 		}
	// 	}
	// 	List<AreaElevationIsland> mergedIslands = new List<AreaElevationIsland>();
	// 	for (int i = 0; i < islands.Count; i++) {
	// 		AreaElevationIsland island = islands[i];
	// 		if (island.tilesInIsland.Count > 0) {
	// 			mergedIslands.Add(island);
	// 		}
	// 	}
	// 	return mergedIslands;
	// }

	#region Cellular Automata
	// private IEnumerator GenerateElevationMap(AreaElevationIsland island, LocationStructure elevationStructure) {
	// 	List<LocationGridTile> locationGridTiles = new List<LocationGridTile>();
	// 	for (int i = 0; i < island.tilesInIsland.Count; i++) {
	// 		Area tileInIsland = island.tilesInIsland[i];
	// 		locationGridTiles.AddRange(tileInIsland.gridTileComponent.gridTiles);
	// 	}
	//
	// 	if (island.elevation == ELEVATION.WATER) {
	// 		// yield return MapGenerator.Instance.StartCoroutine(WaterCellAutomata(locationGridTiles, elevationStructure));
	// 	} else if (island.elevation == ELEVATION.MOUNTAIN) {
	// 		// yield return MapGenerator.Instance.StartCoroutine(MountainCellAutomata(locationGridTiles, elevationStructure, island));
	// 	}
	// 	yield return null;
	// }
	// private IEnumerator WaterCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure) {
	// 	LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(locationGridTiles);
	// 	int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, 1, 20); //2
	// 	
	// 	Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
	// 	
	// 	MapGenerator.Instance.StartCoroutine(CellularAutomataGenerator.DrawElevationMapCoroutine(tileMap, cellMap, null, InnerMapManager.Instance.assetManager.shoreTile, 
	// 		null, (locationGridTile) => SetAsWater(locationGridTile, elevationStructure)));
	//
	// 	//create water wells
	// 	int westMost = elevationStructure.tiles.Min(t => t.localPlace.x);
	// 	int eastMost = elevationStructure.tiles.Max(t => t.localPlace.x);
	// 	int southMost = elevationStructure.tiles.Min(t => t.localPlace.y);
	// 	int northMost = elevationStructure.tiles.Max(t => t.localPlace.y);
	// 	
	// 	LocationGridTile northTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.y == northMost && t.tileObjectComponent.objHere == null));
	// 	CreateFishingSpot(northTile);
	// 	
	// 	LocationGridTile southTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.y == southMost && t.tileObjectComponent.objHere == null));
	// 	CreateFishingSpot(southTile);
	// 	
	// 	LocationGridTile westTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.x == westMost && t.tileObjectComponent.objHere == null));
	// 	CreateFishingSpot(westTile);
	// 	
	// 	LocationGridTile eastTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.x == eastMost && t.tileObjectComponent.objHere == null));
	// 	CreateFishingSpot(eastTile);
	//
	// 	Area occupiedArea = elevationStructure.tiles.ElementAt(0).area;
	// 	elevationStructure.SetOccupiedArea(occupiedArea);
	// 	
	// 	yield return null;
	// }
	private void CreateFishingSpot(LocationGridTile tile) {
		if (tile != null) {
			TileObject well = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.FISHING_SPOT);
			tile.structure.AddPOI(well, tile);
			well.mapObjectVisual.SetVisual(null);	
		}
	}
	private void SetAsWater(LocationGridTile tile, LocationStructure structure) {
		tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		tile.SetStructure(structure);
		tile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(tile.tileObjectComponent.genericTileObject, "Wet", overrideDuration: 0);
	}
	private bool ShouldTileBePartOfMountain(LocationGridTile p_tile, List<LocationGridTile> locationGridTiles) {
		if (p_tile.HasNeighbourNotInList(locationGridTiles)) {
			return false;
		}
		if (p_tile.IsAtEdgeOfMap()) {
			return false;
		}
		return true;
	}
	// private IEnumerator MountainCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure, AreaElevationIsland areaElevationIsland) {
	// 	List<LocationGridTile> refinedTiles = locationGridTiles.Where(t => ShouldTileBePartOfMountain(t, locationGridTiles)).ToList();
	// 	
	// 	LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(refinedTiles);
	// 	int fillPercent = 12;
	// 	int smoothing = 2;
	// 	if (areaElevationIsland.tilesInIsland.Count > 1) { 
	// 		fillPercent = 30;
	// 		smoothing = 2;
	// 	}
	// 	int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, refinedTiles, smoothing, fillPercent);
	// 	
	// 	Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
	// 	
	// 	yield return MapGenerator.Instance.StartCoroutine(CellularAutomataGenerator.DrawElevationMapCoroutine(tileMap, cellMap, InnerMapManager.Instance.assetManager.caveWallTile, 
	// 		null, 
	// 		(locationGridTile) => SetAsMountainWall(locationGridTile, elevationStructure),
	// 		(locationGridTile) => SetAsMountainGround(locationGridTile, elevationStructure)));
	//
	// 	for (int i = 0; i < areaElevationIsland.tilesInIsland.Count; i++) {
	// 		Area tile = areaElevationIsland.tilesInIsland[i];
	// 		LocationGridTile randomTile = tile.gridTileComponent.centerGridTile;
	// 		for (int j = 0; j < tile.neighbourComponent.neighbours.Count; j++) {
	// 			Area neighbour = tile.neighbourComponent.neighbours[j];
	// 			if (areaElevationIsland.tilesInIsland.Contains(neighbour)) {
	// 				LocationGridTile targetTile = neighbour.gridTileComponent.centerGridTile;
	// 				bool hasPath = PathGenerator.Instance.GetPath(randomTile, targetTile, GRID_PATHFINDING_MODE.NORMAL) != null;
	// 				if (hasPath) {
	// 					continue; //already has path towards center of neighbour, skip.
	// 				}
	// 				//neighbour is part of elevation island, make path towards each other
	// 				List<LocationGridTile> path = PathGenerator.Instance.GetPath(randomTile, targetTile, GRID_PATHFINDING_MODE.CAVE_INTERCONNECTION);
	// 				if (path != null) {
	// 					for (int k = 0; k < path.Count; k++) {
	// 						LocationGridTile pathTile = path[k];
	// 						if (pathTile.tileObjectComponent.objHere is BlockWall) {
	// 							pathTile.structure.RemovePOI(pathTile.tileObjectComponent.objHere);
	// 						}		
	// 					}	
	// 				}
	// 			}
	// 		}
	// 		yield return null;
	// 	}
	// 	Area occupiedArea = elevationStructure.tiles.ElementAt(0).area;
	// 	elevationStructure.SetOccupiedArea(occupiedArea);
	//
	// 	List<BlockWall> validWallsForOreVeins = RuinarchListPool<BlockWall>.Claim();
	// 	elevationStructure.PopulateTileObjectsOfTypeThatIsBlockWallValidForOreVein(validWallsForOreVeins);
	//
	// 	var randomOreAmount = areaElevationIsland.tilesInIsland.Count == 1 ? UnityEngine.Random.Range(4, 11) : UnityEngine.Random.Range(8, 16);
	// 	for (int i = 0; i < randomOreAmount; i++) {
	// 		if (validWallsForOreVeins.Count == 0) { break; }
	// 		BlockWall blockWall = CollectionUtilities.GetRandomElement(validWallsForOreVeins);
	// 		CreateOreVeinAt(blockWall.gridTileLocation);
	// 		validWallsForOreVeins.Remove(blockWall);
	// 	}
	// 	RuinarchListPool<BlockWall>.Release(validWallsForOreVeins);
	// }
	//private bool IsBlockWallValidForOreVein(BlockWall p_blockWall) {
	//	if (p_blockWall.gridTileLocation != null) {
	//		int caveNeighbours = p_blockWall.gridTileLocation.neighbourList.Count(t => t.tileObjectComponent.objHere is BlockWall);
	//		if (caveNeighbours == 2 || caveNeighbours == 5) {
	//			return p_blockWall.gridTileLocation.neighbourList.Count(t => t.structure is Wilderness) >= 3;	
	//		}
	//	}
	//	return false;
	//}
	// private void CreateOreVeinAt(LocationGridTile tile) {
	// 	if (tile != null) {
	// 		if (tile.tileObjectComponent.objHere != null) {
	// 			tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
	// 		}
	// 		TileObject well = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ORE_VEIN);
	// 		tile.structure.AddPOI(well, tile);
	// 	}
	// }
	// private void SetAsMountainWall(LocationGridTile tile, LocationStructure structure) {
	// 	tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
	// 	tile.SetTileType(LocationGridTile.Tile_Type.Wall);
	// 	tile.SetTileState(LocationGridTile.Tile_State.Occupied);
	// 	tile.SetStructure(structure);
	// 	
	// 	//create wall tile object
	// 	BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
	// 	blockWall.SetWallType(WALL_TYPE.Stone);
	// 	structure.AddPOI(blockWall, tile);
	// 	tile.SetIsDefault(false);
	// }
	// private void SetAsMountainGround(LocationGridTile tile, LocationStructure structure) {
	// 	tile.SetStructure(structure);
	// 	tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
	// 	tile.SetIsDefault(false);
	// }
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
}

public class AreaElevationIsland {
	public readonly ELEVATION elevation;
	public readonly List<Area> tilesInIsland;

	public AreaElevationIsland(ELEVATION elevation) {
		this.elevation = elevation;
		tilesInIsland = new List<Area>();
	}

	public void AddTile(Area tile) {
		if (tilesInIsland.Contains(tile) == false) {
			tilesInIsland.Add(tile);	
		}
	}
	private void RemoveAllTiles() {
		tilesInIsland.Clear();
	}
	
	public void MergeWithIsland(AreaElevationIsland otherIsland) {
		for (int i = 0; i < otherIsland.tilesInIsland.Count; i++) {
			Area tileInOtherIsland = otherIsland.tilesInIsland[i];
			AddTile(tileInOtherIsland);
		}
		otherIsland.RemoveAllTiles();
	}

	public bool IsAdjacentToIsland(AreaElevationIsland otherIsland) {
		for (int i = 0; i < tilesInIsland.Count; i++) {
			Area tile = tilesInIsland[i];
			for (int j = 0; j < tile.neighbourComponent.neighbours.Count; j++) {
				Area neighbour = tile.neighbourComponent.neighbours[j];
				if (otherIsland.tilesInIsland.Contains(neighbour)) {
					//this island has a tile that has a neighbour that is part of the given island.
					return true;
				}
			}
		}
		return false;
	}
}
