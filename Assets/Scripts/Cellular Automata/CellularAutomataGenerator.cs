using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
namespace Cellular_Automata {
	public static class CellularAutomataGenerator {

		public static int[,] GenerateMap(LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, 
			int smoothing, int randomFillPercent, string seed = "", bool edgesAreAlwaysWalls = true) {
			
			int width = tileMap.GetUpperBound(0) + 1;
			int height = tileMap.GetUpperBound(1) + 1;
			
			int[,] map = new int[width, height];
			RandomFillMap(width, height, map, randomFillPercent, tileMap, allTiles, seed, edgesAreAlwaysWalls);
			
			for (int i = 0; i < smoothing; i++) {
				SmoothMap(map, width, height, tileMap, allTiles, edgesAreAlwaysWalls);	
			}
			return map;
		}
		private static void RandomFillMap(int width, int height, int[,] map, int randomFillPercent, 
			LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, string seed, 
			bool edgesAreAlwaysWalls = true) {
			if (string.IsNullOrEmpty(seed)) {
				seed = Time.time.ToString();
			}
			
			System.Random pseudoRandom = new System.Random(seed.GetHashCode());
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					LocationGridTile tile = tileMap[x, y];
					if (edgesAreAlwaysWalls && (tile == null || IsAtEdgeOfMap(tile, allTiles))) {
						//wall
						map[x, y] = 1;
					} else {
						//random
						map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;	
					}
				}	
			}
		}
		private static void SmoothMap(int[,] map, int width, int height, LocationGridTile[,] tileMap, 
			List<LocationGridTile> allTiles, bool edgesAreAlwaysWalls = true) {
			int[,] tempMap = map;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					LocationGridTile tile = tileMap[x, y];
					if (edgesAreAlwaysWalls && tile != null && IsAtEdgeOfMap(tile, allTiles)) {
						tempMap[x, y] = 1;
					} else {
						int neighbourWallTiles = GetSurroundingWallCount(map, x, y, width, height);
						if (neighbourWallTiles > 4) {
							tempMap[x, y] = 1;
						} else if (neighbourWallTiles < 4) {
							tempMap[x, y] = 0;
						}	
					}
					
					
				}	
			}
			map = tempMap;
		}

		private static int GetSurroundingWallCount(int[,] map, int gridX, int gridY, int width, int height) {
			int wallCount = 0;
			for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
				for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
					if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
						if (neighbourX != gridX || neighbourY != gridY) {
							wallCount += map[neighbourX, neighbourY];
						}	
					} else {
						wallCount++;
					}
				}
			}
			return wallCount;
		}
		// private void OnDrawGizmos() {
		// 	if (map != null) {
		// 		for (int x = 0; x < width; x++) {
		// 			for (int y = 0; y < height; y++) {
		// 				Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
		// 				Vector3 pos = new Vector3(-width/2 + x + 0.5f, 0, -height/2 + y + 0.5f);
		// 				Gizmos.DrawCube(pos, Vector3.one);
		// 			}	
		// 		}	
		// 	}
		// }

		public static bool IsAtEdgeOfMap(LocationGridTile tile, List<LocationGridTile> allTiles) {
			return tile.HasNeighbourNotInList(allTiles) || tile.IsAtEdgeOfMap();
		}
		
		public static LocationGridTile[,] ConvertListToGridMap(List<LocationGridTile> locationGridTiles) {
			int minX = locationGridTiles.Min(t => t.localPlace.x);
			int maxX = locationGridTiles.Max(t => t.localPlace.x);
			int minY = locationGridTiles.Min(t => t.localPlace.y);
			int maxY = locationGridTiles.Max(t => t.localPlace.y);

			int xSize = (maxX - minX) + 1;
			int ySize = (maxY - minY) + 1;
			
			LocationGridTile[,] arrangedMap = new LocationGridTile[xSize, ySize];
			for (int i = 0; i < locationGridTiles.Count; i++) {
				LocationGridTile tile = locationGridTiles[i];
				int localX = tile.localPlace.x - minX;
				int localY = tile.localPlace.y - minY;
				arrangedMap[localX, localY] = tile;
			}

			return arrangedMap;
		}

		/// <summary>
		/// Draw map for given tiles based on given tile map and cellular
		/// automata generated data.
		/// </summary>
		/// <param name="tileMap">2D array of LocationGridTiles to change</param>
		/// <param name="cellAutomata">2D array of generated settings using cellular automata</param>
		/// <param name="wallAsset">Asset to use if tile is set as a wall.</param>
		/// <param name="groundAsset">Asset to use if tile is set as ground</param>
		/// <param name="wallAction">Action to perform to the tile when it is set as a wall.</param>
		/// <param name="groundAction">Action to perform to the tile when it is set as ground.</param>
		public static void DrawMap(LocationGridTile[,] tileMap, int[,] cellAutomata, TileBase wallAsset,
			TileBase groundAsset, System.Action<LocationGridTile> wallAction, System.Action<LocationGridTile> groundAction) {
			Assert.IsTrue(tileMap.GetUpperBound(0) == cellAutomata.GetUpperBound(0),
				$"Provided tile map and cell map have inconsistent first dimension bounds. {tileMap.GetUpperBound(0).ToString()}/{cellAutomata.GetUpperBound(0).ToString()}");
			Assert.IsTrue(tileMap.GetUpperBound(1) == cellAutomata.GetUpperBound(1),
				$"Provided tile map and cell map have inconsistent first dimension bounds. {tileMap.GetUpperBound(1).ToString()}/{cellAutomata.GetUpperBound(1).ToString()}");

			int upperBoundX = tileMap.GetUpperBound(0) + 1;
			int upperBoundY = tileMap.GetUpperBound(1) + 1;
			for (int x = 0; x < upperBoundX; x++) {
				for (int y = 0; y < upperBoundY; y++) {
					LocationGridTile tile = tileMap[x, y];
					int cellMapValue = cellAutomata[x, y];
					if (tile != null) {
						if (cellMapValue == 1) {
							//wall
							tile.SetStructureTilemapVisual(wallAsset);
							wallAction?.Invoke(tile);
						} else {
							//ground	
							tile.SetStructureTilemapVisual(groundAsset);
							groundAction?.Invoke(tile);
						}
					}
				}	
			}

		}
	}	
}
