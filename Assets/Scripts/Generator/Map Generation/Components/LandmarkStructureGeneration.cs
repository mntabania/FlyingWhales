using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class LandmarkStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarks();
		for (int i = 0; i < landmarks.Count; i++) {
			BaseLandmark landmark = landmarks[i];
			if (landmark.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR) {
				LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(landmark.tileLocation.region,
					LandmarkManager.Instance.GetStructureTypeFor(landmark.specificLandmarkType));
				landmark.tileLocation.settlementOnTile.GenerateStructures(structure);
				yield return MapGenerator.Instance.StartCoroutine(
					GenerateMonsterLair(landmark.tileLocation, structure));
			} else if (landmark.specificLandmarkType != LANDMARK_TYPE.VILLAGE) {
				yield return MapGenerator.Instance.StartCoroutine(CreateStructureObjectForLandmark(landmark, data));	
			}
		}
		yield return null;
	}

	private IEnumerator CreateStructureObjectForLandmark(BaseLandmark landmark, MapGenerationData data) {
		LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(landmark.tileLocation.region,
			LandmarkManager.Instance.GetStructureTypeFor(landmark.specificLandmarkType));
		if (structure.structureType == STRUCTURE_TYPE.THE_PORTAL) {
			data.portalStructure = structure;
			landmark.tileLocation.InstantlyCorruptAllOwnedInnerMapTiles();
		}
		yield return MapGenerator.Instance.StartCoroutine(PlaceInitialStructure(structure, landmark.tileLocation.region.innerMap, landmark.tileLocation));
	}

	private IEnumerator PlaceInitialStructure(LocationStructure structure, InnerTileMap innerTileMap, HexTile tile) {
		if (structure.structureType.ShouldBeGeneratedFromTemplate()) {
			List<GameObject> choices =
				InnerMapManager.Instance.GetStructurePrefabsForStructure(structure.structureType);
			GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
			LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
			if (LandmarkManager.Instance.PlayerTryGetBuildSpotForStructureInTile(lso, tile, innerTileMap, out var chosenBuildingSpot)) {
				innerTileMap.PlaceStructureObjectAt(chosenBuildingSpot, chosenStructurePrefab, structure);
				structure.structureObj.RegisterPreplacedObjects(structure, innerTileMap);
			} else {
				throw new System.Exception(
					$"Could not find valid building spot for {structure.ToString()} using prefab {chosenStructurePrefab.name}");
			}
			yield return null;
		}

	}
	
	#region Cellular Automata
	private IEnumerator GenerateMonsterLair(HexTile hexTile, LocationStructure structure) {
		List<LocationGridTile> locationGridTiles = new List<LocationGridTile>(hexTile.locationGridTiles);

		LocationStructure wilderness = hexTile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		
		MonsterLairCellAutomata(locationGridTiles, structure, hexTile.region, wilderness);
		
		for (int j = 0; j < hexTile.ownedBuildSpots.Length; j++) {
			BuildingSpot spot = hexTile.ownedBuildSpots[j];
			if (spot.isOccupied == false) {
				spot.SetIsOccupied(true);
				spot.UpdateAdjacentSpotsOccupancy(hexTile.region.innerMap);	
			}
		}
		
		yield return null;
	}
	private void MonsterLairCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure structure, Region region, LocationStructure wilderness) {
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(locationGridTiles);
		int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, 2, 15);
		
		Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {structure.ToString()}");
		
		CellularAutomataGenerator.DrawMap(tileMap, cellMap, InnerMapManager.Instance.assetManager.monsterLairWallTile, 
			null, 
			(locationGridTile) => SetAsWall(locationGridTile, structure),
			(locationGridTile) => SetAsGround(locationGridTile, structure));

		List<LocationGridTile> tilesToRefine = new List<LocationGridTile>(locationGridTiles);
		//refine further
		for (int i = 0; i < tilesToRefine.Count; i++) {
			LocationGridTile tile = tilesToRefine[i];
			if (tile.HasNeighbourOfType(LocationGridTile.Ground_Type.Flesh) == false) {
				tile.SetStructureTilemapVisual(null);
				tile.SetTileType(LocationGridTile.Tile_Type.Empty);
				tile.SetTileState(LocationGridTile.Tile_State.Empty);
				tile.RevertToPreviousGroundVisual();
				tile.SetStructure(wilderness);
				locationGridTiles.Remove(tile);
			}
		}
		MonsterLairPerlin(locationGridTiles, structure);
		//create entrances
		//get tiles that are at the edge of the given tiles, but are not at the edge of its map.
		for (int i = 0; i < 5; i++) {
			List<LocationGridTile> targetChoices = locationGridTiles
				.Where(t => t.tileType == LocationGridTile.Tile_Type.Wall 
				            && t.IsAtEdgeOfMap() == false
				            && t.HasDifferentStructureNeighbour(true)
				            && t.GetCountNeighboursOfType(LocationGridTile.Tile_Type.Wall, true) == 2 
				            && t.GetCountNeighboursOfType(LocationGridTile.Tile_Type.Empty, true) == 2).ToList();
			if (targetChoices.Count > 0) {
				LocationGridTile target = CollectionUtilities.GetRandomElement(targetChoices);
				// Debug.Log($"Chosen target tile to clear is {target.ToString()} for monster lair at {region.name}");
				target.SetStructureTilemapVisual(null);
				target.SetTileType(LocationGridTile.Tile_Type.Empty);
				target.SetStructure(wilderness);
				target.RevertToPreviousGroundVisual();
				locationGridTiles.Remove(target);
			}
			else {
				Debug.LogWarning($"Could not find entrance for {structure}");
				break;
			}
		}
		
		

		for (int i = 0; i < locationGridTiles.Count; i++) {
			LocationGridTile tile = locationGridTiles[i];
			if (tile.tileType == LocationGridTile.Tile_Type.Wall) {
				//create wall tile object for all walls
				BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
				blockWall.SetWallType(WALL_TYPE.Flesh);
				structure.AddPOI(blockWall, tile);
			}
		}
	}
	private void SetAsWall(LocationGridTile tile, LocationStructure structure) {
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.monsterLairGroundTile);	
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		tile.SetStructure(structure);
	}
	private void SetAsGround(LocationGridTile tile, LocationStructure structure) {
		tile.SetStructure(structure);
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.monsterLairGroundTile);
		// tile.SetStructure(structure);
	}
	private void MonsterLairPerlin(List<LocationGridTile> tiles, LocationStructure structure) {
		float offsetX = UnityEngine.Random.Range(0f, 99999f);
		float offsetY = UnityEngine.Random.Range(0f, 99999f);
		int minX = tiles.Min(t => t.localPlace.x);
		int maxX = tiles.Max(t => t.localPlace.x);
		int minY = tiles.Min(t => t.localPlace.y);
		int maxY = tiles.Max(t => t.localPlace.y);
		int xSize = maxX - minX;
		int ySize = maxY - minY;
		for (int i = 0; i < tiles.Count; i++) {
			LocationGridTile currTile = tiles[i];
			float xCoord = (float) currTile.localPlace.x / xSize * 11f + offsetX;
			float yCoord = (float) currTile.localPlace.y / ySize * 11f + offsetY;

			float floorSample = Mathf.PerlinNoise(xCoord, yCoord);
			if (floorSample <= 0.45f) {
				SetAsWall(currTile, structure);
			}
		}
	}
	#endregion
}
