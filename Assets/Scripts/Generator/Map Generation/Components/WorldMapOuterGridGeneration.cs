﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenario_Maps;
using UnityEngine;

// public class WorldMapOuterGridGeneration : MapGenerationComponent {
//     
//    private const int _borderThickness = 4;
//    
// 	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
// 		yield return MapGenerator.Instance.StartCoroutine(GenerateOuterGrid(data));
// 		Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allAreas);
//         Biomes.Instance.UpdateTileVisuals(GridMap.Instance.outerGridList.ToList());
// 		// CameraMove.Instance.CalculateCameraBounds();
// 		yield return null;
// 	}
//
//     #region Scenario Maps
//     public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
//         yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
//     }
//     #endregion
//     
//     #region Saved World
//     public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
//         yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
//     }
//     #endregion
//     
//     internal IEnumerator GenerateOuterGrid(MapGenerationData data) {
//         int newWidth = data.width + (_borderThickness * 2);
//         int newHeight = data.height + (_borderThickness * 2);
//
//         float newX = MapGenerationData.XOffset * (newWidth / 2f);
//         float newY = MapGenerationData.YOffset * (newHeight / 2f);
//
//         int id = 0;
//
//         HashSet<HexTile> outerGridList = new HashSet<HexTile>();
//         GridMap.Instance.borderParent.transform.localPosition = new Vector2(-newX, -newY);
//
//         int batchCount = 0;
//         
//         for (int x = 0; x < newWidth; x++) {
//             for (int y = 0; y < newHeight; y++) {
//                 if ((x >= _borderThickness && x < newWidth - _borderThickness) && (y >= _borderThickness && y < newHeight - _borderThickness)) {
//                     continue;
//                 }
//                 float xPosition = x * MapGenerationData.XOffset;
//
//                 float yPosition = y * MapGenerationData.YOffset;
//                 if (y % 2 == 1) {
//                     xPosition += MapGenerationData.XOffset / 2;
//                 }
//
//                 GameObject hex = Object.Instantiate(GridMap.Instance.goHex, GridMap.Instance.borderParent, true) as GameObject;
//                 hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
//                 hex.transform.localScale = new Vector3(MapGenerationData.TileSize, MapGenerationData.TileSize, 0f);
//                 HexTile currHex = hex.GetComponent<HexTile>();
//                 currHex.Initialize(false);
//                 currHex.data.id = id;
//                 currHex.data.tileName = hex.name;
//                 currHex.data.xCoordinate = x - _borderThickness;
//                 currHex.data.yCoordinate = y - _borderThickness;
//                 
//                 outerGridList.Add(currHex);
//
//                 HexTile hexToCopy = GetTileToCopy(x, y, data.width, data.height);
//                 currHex.name = $"{currHex.xCoordinate.ToString()},{currHex.yCoordinate.ToString()}(Border) Copied from {hexToCopy.name}";
//
//                 currHex.SetElevation(ELEVATION.WATER);
//                 Biomes.Instance.SetBiomeForTile(hexToCopy.biomeType, currHex);
//
//                 currHex.DisableColliders();
//                 id++;
//                 
//                 batchCount++;
//                 if (batchCount == MapGenerationData.WorldMapOuterGridGenerationBatches) {
//                     batchCount = 0;
//                     yield return null;    
//                 }
//             }
//         }
//
//         GridMap.Instance.SetOuterGridList(outerGridList);
//
//         batchCount = 0;
//         for (int i = 0; i < outerGridList.Count; i++) {
//             HexTile tile = outerGridList.ElementAt(i);
//             tile.FindNeighboursForBorders();
//             batchCount++;
//             if (batchCount == MapGenerationData.WorldMapOuterGridGenerationBatches) {
//                 batchCount = 0;
//                 yield return null;    
//             }
//         }
//     }
//
//     private HexTile GetTileToCopy(int x, int y, int width, int height) {
//         int xToCopy;
//         int yToCopy;
//         if (x < _borderThickness && y - _borderThickness >= 0 && y < height) { //if border thickness is 2 (0 and 1)
//             //left border
//             xToCopy = 0;
//             yToCopy = y - _borderThickness;
//         } else if (x >= _borderThickness && x <= width && y < _borderThickness) {
//             //bottom border
//             xToCopy = x - _borderThickness;
//             yToCopy = 0;
//         } else if (x > width && (y - _borderThickness >= 0 && y - _borderThickness <= height - 1)) {
//             //right border
//             xToCopy = width - 1;
//             yToCopy = y - _borderThickness;
//         } else if (x >= _borderThickness && x <= width && y - _borderThickness >= height) {
//             //top border
//             xToCopy = x - _borderThickness;
//             yToCopy = height - 1;
//         } else {
//             //corners
//             xToCopy = x;
//             yToCopy = y;
//             xToCopy = Mathf.Clamp(xToCopy, 0, width - 1);
//             yToCopy = Mathf.Clamp(yToCopy, 0, height - 1);
//         }
//
//         return GridMap.Instance.map[xToCopy, yToCopy];
//     }
//     
// }
