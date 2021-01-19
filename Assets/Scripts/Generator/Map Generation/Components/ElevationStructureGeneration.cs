﻿using System;
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
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			List<ElevationIsland> islandsInRegion = GetElevationIslandsInRegion(region);
			for (int j = 0; j < islandsInRegion.Count; j++) {
				ElevationIsland currIsland = islandsInRegion[j];
				STRUCTURE_TYPE structureType = GetStructureTypeFor(currIsland.elevation);
				NPCSettlement settlement = null;
				if (structureType == STRUCTURE_TYPE.CAVE) {
					//only create settlement for caves
					settlement = LandmarkManager.Instance.CreateNewSettlement(region, LOCATION_TYPE.DUNGEON, currIsland.tilesInIsland.ToArray());	
				}
				LocationStructure elevationStructure = LandmarkManager.Instance.CreateNewStructureAt(region, structureType, settlement);
				
				yield return MapGenerator.Instance.StartCoroutine(GenerateElevationMap(currIsland, elevationStructure));
				yield return MapGenerator.Instance.StartCoroutine(RefreshTilemapCollider(region.innerMap.structureTilemapCollider));
			}
		}
		yield return null;
	}
	private IEnumerator RefreshTilemapCollider(TilemapCollider2D tilemapCollider2D) {
		tilemapCollider2D.enabled = false;
		yield return new WaitForSeconds(0.5f);
		// ReSharper disable once Unity.InefficientPropertyAccess
		tilemapCollider2D.enabled = true;
	}
	private STRUCTURE_TYPE GetStructureTypeFor(ELEVATION elevation) {
		switch (elevation) {
			case ELEVATION.MOUNTAIN:
				return STRUCTURE_TYPE.CAVE;
			case ELEVATION.WATER:
				return STRUCTURE_TYPE.OCEAN;
		}
		throw new Exception($"There is no corresponding structure type for {elevation.ToString()}");
	}
	private List<ElevationIsland> GetElevationIslandsInRegion(Region region) {
		List<ElevationIsland> islands = new List<ElevationIsland>();
		ELEVATION[] elevationsToCheck = new[] {ELEVATION.WATER, ELEVATION.MOUNTAIN};
		for (int i = 0; i < elevationsToCheck.Length; i++) {
			ELEVATION elevation = elevationsToCheck[i];
			List<HexTile> tilesOfThatElevation = GetTilesWithElevationInRegion(region, elevation);
			List<ElevationIsland> initialIslands = CreateInitialIslands(tilesOfThatElevation, elevation);
			List<ElevationIsland> mergedIslands = MergeIslands(initialIslands);
			islands.AddRange(mergedIslands);
		}
		return islands;
	}
	private List<HexTile> GetTilesWithElevationInRegion(Region region, ELEVATION elevation) {
		List<HexTile> tiles = new List<HexTile>();
		for (int i = 0; i < region.tiles.Count; i++) {
			HexTile tile = region.tiles[i];
			if (tile.elevationType == elevation) {
				tiles.Add(tile);
			}
		}
		return tiles;
	}
	private List<ElevationIsland> CreateInitialIslands(List<HexTile> tiles, ELEVATION elevation) {
		List<ElevationIsland> islands = new List<ElevationIsland>();
		for (int i = 0; i < tiles.Count; i++) {
			HexTile tile = tiles[i];
			ElevationIsland island = new ElevationIsland(elevation);
			island.AddTile(tile);
			islands.Add(island);
		}
		return islands;
	}
	private List<ElevationIsland> MergeIslands(List<ElevationIsland> islands) {
		for (int i = 0; i < islands.Count; i++) {
			ElevationIsland currIsland = islands[i];
			for (int j = 0; j < islands.Count; j++) {
				ElevationIsland otherIsland = islands[j];
				if (currIsland != otherIsland) {
					if (currIsland.IsAdjacentToIsland(otherIsland)) {
						currIsland.MergeWithIsland(otherIsland);
					}
				}
			}
		}
		List<ElevationIsland> mergedIslands = new List<ElevationIsland>();
		for (int i = 0; i < islands.Count; i++) {
			ElevationIsland island = islands[i];
			if (island.tilesInIsland.Count > 0) {
				mergedIslands.Add(island);
			}
		}
		return mergedIslands;
	}

	#region Cellular Automata
	private IEnumerator GenerateElevationMap(ElevationIsland island, LocationStructure elevationStructure) {
		List<LocationGridTile> locationGridTiles = new List<LocationGridTile>();
		for (int i = 0; i < island.tilesInIsland.Count; i++) {
			HexTile tileInIsland = island.tilesInIsland[i];
			locationGridTiles.AddRange(tileInIsland.locationGridTiles);
		}

		if (island.elevation == ELEVATION.WATER) {
			yield return MapGenerator.Instance.StartCoroutine(WaterCellAutomata(locationGridTiles, elevationStructure));
		} else if (island.elevation == ELEVATION.MOUNTAIN) {
			yield return MapGenerator.Instance.StartCoroutine(MountainCellAutomata(locationGridTiles, elevationStructure, island));
		}

		for (int i = 0; i < island.tilesInIsland.Count; i++) {
			HexTile hexTile = island.tilesInIsland[i];
			hexTile.innerMapHexTile.Occupy();
			// for (int j = 0; j < hexTile.ownedBuildSpots.Length; j++) {
			// 	BuildingSpot spot = hexTile.ownedBuildSpots[j];
			// 	if (spot.isOccupied == false) {
			// 		spot.SetIsOccupied(true);
			// 		spot.UpdateAdjacentSpotsOccupancy(hexTile.region.innerMap);	
			// 	}
			// }
		}
		yield return null;
	}
	private IEnumerator WaterCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure) {
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(locationGridTiles);
		int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, 1, 20); //2
		
		Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
		
		MapGenerator.Instance.StartCoroutine(CellularAutomataGenerator.DrawMapCoroutine(tileMap, cellMap, null, InnerMapManager.Instance.assetManager.shoreTile, 
			null, (locationGridTile) => SetAsWater(locationGridTile, elevationStructure)));

		//create water wells
		int westMost = elevationStructure.tiles.Min(t => t.localPlace.x);
		int eastMost = elevationStructure.tiles.Max(t => t.localPlace.x);
		int southMost = elevationStructure.tiles.Min(t => t.localPlace.y);
		int northMost = elevationStructure.tiles.Max(t => t.localPlace.y);
		
		LocationGridTile northTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.y == northMost && t.objHere == null));
		CreateFishingSpot(northTile);
		
		LocationGridTile southTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.y == southMost && t.objHere == null));
		CreateFishingSpot(southTile);
		
		LocationGridTile westTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.x == westMost && t.objHere == null));
		CreateFishingSpot(westTile);
		
		LocationGridTile eastTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.x == eastMost && t.objHere == null));
		CreateFishingSpot(eastTile);
		
		yield return null;
	}
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
		tile.genericTileObject.traitContainer.AddTrait(tile.genericTileObject, "Wet", overrideDuration: 0);
	}
	private IEnumerator MountainCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure, ElevationIsland elevationIsland) {
		List<LocationGridTile> refinedTiles = locationGridTiles.Where(t => t.HasNeighbourNotInList(locationGridTiles) == false && t.IsAtEdgeOfMap() == false).ToList();
		
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(refinedTiles);
		int fillPercent = 12;
		int smoothing = 2;
		if (elevationIsland.tilesInIsland.Count > 1) { 
			fillPercent = 30;
			smoothing = 2;
		}
		int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, refinedTiles, smoothing, fillPercent);
		
		Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
		
		yield return MapGenerator.Instance.StartCoroutine(CellularAutomataGenerator.DrawMapCoroutine(tileMap, cellMap, InnerMapManager.Instance.assetManager.caveWallTile, 
			null, 
			(locationGridTile) => SetAsMountainWall(locationGridTile, elevationStructure),
			(locationGridTile) => SetAsMountainGround(locationGridTile, elevationStructure)));

		for (int i = 0; i < elevationIsland.tilesInIsland.Count; i++) {
			HexTile tile = elevationIsland.tilesInIsland[i];
			LocationGridTile randomTile = tile.GetCenterLocationGridTile();
			for (int j = 0; j < tile.AllNeighbours.Count; j++) {
				HexTile neighbour = tile.AllNeighbours[j];
				if (elevationIsland.tilesInIsland.Contains(neighbour)) {
					LocationGridTile targetTile = neighbour.GetCenterLocationGridTile();
					bool hasPath = PathGenerator.Instance.GetPath(randomTile, targetTile, GRID_PATHFINDING_MODE.NORMAL) != null;
					if (hasPath) {
						continue; //already has path towards center of neighbour, skip.
					}
					//neighbour is part of elevation island, make path towards each other
					List<LocationGridTile> path = PathGenerator.Instance.GetPath(randomTile, targetTile, GRID_PATHFINDING_MODE.CAVE_INTERCONNECTION);
					if (path != null) {
						for (int k = 0; k < path.Count; k++) {
							LocationGridTile pathTile = path[k];
							if (pathTile.objHere is BlockWall) {
								pathTile.structure.RemovePOI(pathTile.objHere);
							}		
						}	
					}
				}
			}
			yield return null;
		}

		List<BlockWall> validWallsForOreVeins = elevationStructure.GetTileObjectsOfType<BlockWall>(IsBlockWallValidForOreVein);

		var randomOreAmount = elevationIsland.tilesInIsland.Count == 1 ? UnityEngine.Random.Range(4, 11) : UnityEngine.Random.Range(8, 16);
		for (int i = 0; i < randomOreAmount; i++) {
			if (validWallsForOreVeins.Count == 0) { break; }
			BlockWall blockWall = CollectionUtilities.GetRandomElement(validWallsForOreVeins);
			CreateOreVeinAt(blockWall.gridTileLocation);
			validWallsForOreVeins.Remove(blockWall);
		}
		
		// //create ore veins
		// int westMost = elevationStructure.tiles.Min(t => t.localPlace.x);
		// int eastMost = elevationStructure.tiles.Max(t => t.localPlace.x);
		// int southMost = elevationStructure.tiles.Min(t => t.localPlace.y);
		// int northMost = elevationStructure.tiles.Max(t => t.localPlace.y);
		//
		// LocationGridTile northTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.y == northMost && t.localPlace.x != eastMost && t.localPlace.x != westMost));
		// CreateOreVeinAt(northTile);
		//
		// LocationGridTile southTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.y == southMost && t.localPlace.x != eastMost && t.localPlace.x != westMost));
		// CreateOreVeinAt(southTile);
		//
		// LocationGridTile westTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.x == westMost && t.localPlace.y != northMost && t.localPlace.y != southMost));
		// CreateOreVeinAt(westTile);
		//
		// LocationGridTile eastTile = CollectionUtilities.GetRandomElement(elevationStructure.tiles.Where(t => t.localPlace.x == eastMost && t.localPlace.y != northMost && t.localPlace.y != southMost));
		// CreateOreVeinAt(eastTile);
	}
	private bool IsBlockWallValidForOreVein(BlockWall p_blockWall) {
		if (p_blockWall.gridTileLocation != null) {
			int caveNeighbours = p_blockWall.gridTileLocation.neighbourList.Count(t => t.objHere is BlockWall);
			if (caveNeighbours == 2 || caveNeighbours == 5) {
				return p_blockWall.gridTileLocation.neighbourList.Count(t => t.structure is Wilderness) >= 3;	
			}
		}
		return false;
	}
	private void CreateOreVeinAt(LocationGridTile tile) {
		if (tile != null) {
			if (tile.objHere != null) {
				tile.structure.RemovePOI(tile.objHere);
			}
			TileObject well = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ORE_VEIN);
			tile.structure.AddPOI(well, tile);
		}
	}
	private void SetAsMountainWall(LocationGridTile tile, LocationStructure structure) {
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		tile.SetStructure(structure);
		
		//create wall tile object
		BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
		blockWall.SetWallType(WALL_TYPE.Stone);
		structure.AddPOI(blockWall, tile);
	}
	private void SetAsMountainGround(LocationGridTile tile, LocationStructure structure) {
		tile.SetStructure(structure);
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
	}
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

public class ElevationIsland {
	public readonly ELEVATION elevation;
	public readonly List<HexTile> tilesInIsland;

	public ElevationIsland(ELEVATION elevation) {
		this.elevation = elevation;
		tilesInIsland = new List<HexTile>();
	}

	public void AddTile(HexTile tile) {
		if (tilesInIsland.Contains(tile) == false) {
			tilesInIsland.Add(tile);	
		}
	}
	private void RemoveTile(HexTile tile) {
		tilesInIsland.Remove(tile);
	}
	private void RemoveAllTiles() {
		tilesInIsland.Clear();
	}
	
	public void MergeWithIsland(ElevationIsland otherIsland) {
		for (int i = 0; i < otherIsland.tilesInIsland.Count; i++) {
			HexTile tileInOtherIsland = otherIsland.tilesInIsland[i];
			AddTile(tileInOtherIsland);
		}
		otherIsland.RemoveAllTiles();
	}

	public bool IsAdjacentToIsland(ElevationIsland otherIsland) {
		for (int i = 0; i < tilesInIsland.Count; i++) {
			HexTile tile = tilesInIsland[i];
			for (int j = 0; j < tile.AllNeighbours.Count; j++) {
				HexTile neighbour = tile.AllNeighbours[j];
				if (otherIsland.tilesInIsland.Contains(neighbour)) {
					//this island has a tile that has a neighbour that is part of the given island.
					return true;
				}
			}
		}
		return false;
	}
}
