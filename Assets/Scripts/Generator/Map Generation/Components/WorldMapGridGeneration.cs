using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapGridGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating world map...");
		if (WorldConfigManager.Instance.isDemoWorld) {
			data.regionCount = 1;
			data.width = 8;
			data.height = 10;
		} else {
			data.regionCount = 5;
			data.width = 10;
			data.height = 12;
		}
		
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data));
	}
	private IEnumerator GenerateGrid(MapGenerationData data) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.xOffset * (data.width / 2f);
		float newY = MapGenerationData.yOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		HexTile[,] map = new HexTile[data.width, data.height];
		List<HexTile> normalHexTiles = new List<HexTile>();
		List<HexTile> allTiles = new List<HexTile>();
		int id = 0;

		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				float xPosition = x * MapGenerationData.xOffset;

				float yPosition = y * MapGenerationData.yOffset;
				if (y % 2 == 1) {
					xPosition += MapGenerationData.xOffset / 2;
				}

				GameObject hex = Object.Instantiate(GridMap.Instance.goHex, GridMap.Instance.transform, true) as GameObject;
				hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
				hex.transform.localScale = new Vector3(MapGenerationData.tileSize, MapGenerationData.tileSize, 0f);
				hex.name = $"{x},{y}";
				HexTile currHex = hex.GetComponent<HexTile>();
				currHex.Initialize();
				currHex.data.id = id;
				currHex.data.tileName = RandomNameGenerator.GetTileName();
				currHex.data.xCoordinate = x;
				currHex.data.yCoordinate = y;
				allTiles.Add(currHex);
				normalHexTiles.Add(currHex);
				map[x, y] = hex.GetComponent<HexTile>();
				id++;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, normalHexTiles, allTiles);
		normalHexTiles.ForEach(o => o.FindNeighbours(map));
		yield return null;
	}
}
