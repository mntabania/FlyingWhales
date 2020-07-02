using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Object = UnityEngine.Object;

public class WorldMapGridGeneration : MapGenerationComponent {

	/// <summary>
	/// Dictionary of world templates, grouped by the number of regions.
	/// </summary>
	private Dictionary<int, List<WorldMapTemplate>> worldMapTemplates = new Dictionary<int, List<WorldMapTemplate>>() {
		//1 region
		{ 1, new List<WorldMapTemplate>() 
			{
				new WorldMapTemplate() 
				{
					regionCount = 1,
					worldMapWidth = 8,
					worldMapHeight = 10,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(8, 10), 
							}
						}	
					}	
				}	
			}
		},
		//2 regions
		{ 2, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 2,
					worldMapWidth = 8,
					worldMapHeight = 10,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(4, 10),
								new RegionTemplate(4, 10),  
							}
						}	
					}	
				}
			}
		},
		//3 regions
		{ 3, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 3,
					worldMapWidth = 15,
					worldMapHeight = 8,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(5, 8),
								new RegionTemplate(5, 8),
								new RegionTemplate(5, 8),  
							}
						}	
					}	
				}
			}
		},
		//4 regions
		{ 4, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 4,
					worldMapWidth = 16,
					worldMapHeight = 12,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(8, 6),
								new RegionTemplate(8, 6),
							}
						},
						{ 1, new [] {
								new RegionTemplate(8, 6),
								new RegionTemplate(8, 6),
							}
						}
					}	
				}
			}
		},
		//5 regions
		{ 5, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 5,
					worldMapWidth = 10,
					worldMapHeight = 12,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{0, new[] {
								new RegionTemplate(5, 6),
								new RegionTemplate(5, 6),
							}
						},
						{1, new[] {
								new RegionTemplate(3, 6),
								new RegionTemplate(4, 6),
								new RegionTemplate(3, 6),
							}
						}
					}
				}
			}
		},
		//6 regions
		{ 6, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 6,
					worldMapWidth = 10,
					worldMapHeight = 12,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{0, new[] {
								new RegionTemplate(3, 6),
								new RegionTemplate(4, 6),
								new RegionTemplate(3, 6),
							}
						},
						{1, new[] {
								new RegionTemplate(3, 6),
								new RegionTemplate(4, 6),
								new RegionTemplate(3, 6),
							}
						}
					}
				}
			}
		},
	};
	
	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating world map...");
		int regionCount = WorldSettings.Instance.worldSettingsData.numOfRegions;
		if (WorldConfigManager.Instance.isDemoWorld) {
			regionCount = 1;
		}
		if (worldMapTemplates.ContainsKey(regionCount)) {
			List<WorldMapTemplate> choices = worldMapTemplates[regionCount];
			WorldMapTemplate chosenTemplate = CollectionUtilities.GetRandomElement(choices);
			data.chosenWorldMapTemplate = chosenTemplate;
			Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
			yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data));	
		} else {
			throw new Exception($"No provided world map template for {WorldSettings.Instance.worldSettingsData.numOfRegions.ToString()} regions.");	
		}
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