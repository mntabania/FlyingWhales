using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;
using Object = UnityEngine.Object;

public class WorldMapGridGeneration : MapGenerationComponent {

	/// <summary>
	/// Dictionary of world templates, grouped by the number of regions.
	/// </summary>
	private Dictionary<MAP_SIZE, List<WorldMapTemplate>> worldMapTemplates = new Dictionary<MAP_SIZE, List<WorldMapTemplate>>() {
		//1 region
		{ MAP_SIZE.Small, new List<WorldMapTemplate>() 
			{
				new WorldMapTemplate() 
				{
					regionCount = 1,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(8, 8), 
							}
						}	
					}	
				}	
			}
		},
		//2 regions
		{ MAP_SIZE.Medium, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 2,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(6, 8),
								new RegionTemplate(6, 8),  
							}
						}	
					}	
				}
			}
		},
		//3 regions
		{ MAP_SIZE.Large, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 3,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(5, 10),
								new RegionTemplate(5, 10),
								new RegionTemplate(6, 10),  
							}
						}	
					}	
				}
			}
		},
		//4 regions
		{ MAP_SIZE.Extra_Large, new List<WorldMapTemplate>()
			{
				new WorldMapTemplate()
				{
					regionCount = 4,
					regions = new Dictionary<int, RegionTemplate[]>() {
						{ 0, new [] {
								new RegionTemplate(8, 8),
								new RegionTemplate(8, 8),
							}
						},
						{ 1, new [] {
								new RegionTemplate(8, 8),
								new RegionTemplate(8, 8),
							}
						}
					}	
				}
			}
		},
	};

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating world map...");
		WorldMapTemplate chosenTemplate;
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 1,
				worldMapWidth = 24,
				worldMapHeight = 10,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(24, 10),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 1,
				worldMapWidth = 7,
				worldMapHeight = 8,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(7, 8),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 4,
				worldMapWidth = 14,
				worldMapHeight = 12,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(7, 6),
							new RegionTemplate(7, 6),
						}
					},
					{
						1, new[] {
							new RegionTemplate(7, 6),
							new RegionTemplate(7, 6),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 2,
				worldMapWidth = 10,
				worldMapHeight = 6,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(6, 6),
							new RegionTemplate(4, 6),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 2,
				worldMapWidth = 12,
				worldMapHeight = 10,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(12, 5),
						}
					},
					{
						1, new[] {
							new RegionTemplate(12, 5),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 1,
				worldMapWidth = 13,
				worldMapHeight = 6,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(13, 6),
						}
					},
				}
			};
		} else {
			MAP_SIZE mapSize = WorldSettings.Instance.worldSettingsData.mapSettings.mapSize;
			List<WorldMapTemplate> choices = worldMapTemplates[mapSize];
			WorldMapTemplate randomTemplate = CollectionUtilities.GetRandomElement(choices);
			Vector2 mapVector = WorldSettings.Instance.worldSettingsData.mapSettings.GetMapSize();
			randomTemplate.worldMapWidth = (int)mapVector.x;
			randomTemplate.worldMapHeight = (int)mapVector.y;
			
			chosenTemplate = randomTemplate;
		}

		data.chosenWorldMapTemplate = chosenTemplate;
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data));
	}
	private IEnumerator GenerateGrid(MapGenerationData data) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.XOffset * (data.width / 2f);
		float newY = MapGenerationData.YOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		HexTile[,] map = new HexTile[data.width, data.height];
		List<HexTile> normalHexTiles = new List<HexTile>();
		int id = 0;

		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				float xPosition = x * MapGenerationData.XOffset;

				float yPosition = y * MapGenerationData.YOffset;
				if (y % 2 == 1) {
					xPosition += MapGenerationData.XOffset / 2;
				}

				GameObject hex = Object.Instantiate(GridMap.Instance.goHex, GridMap.Instance.transform, true) as GameObject;
				hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
				hex.transform.localScale = new Vector3(MapGenerationData.TileSize, MapGenerationData.TileSize, 0f);
				hex.name = $"{x},{y}";
				HexTile currHex = hex.GetComponent<HexTile>();
				currHex.Initialize();
				currHex.data.persistentID = System.Guid.NewGuid().ToString();
				currHex.data.id = id;
				currHex.data.tileName = RandomNameGenerator.GetTileName();
				currHex.data.xCoordinate = x;
				currHex.data.yCoordinate = y;
				normalHexTiles.Add(currHex);
				map[x, y] = currHex;
				id++;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, normalHexTiles);
		//Find Neighbours for each hextile
		Parallel.ForEach(normalHexTiles, (hexTile) => {
			hexTile.FindNeighbours(map);
		});
		// for (int i = 0; i < normalHexTiles.Count; i++) {
		// 	HexTile hexTile = normalHexTiles[i];
		// 	hexTile.FindNeighbours(map);
		// }
		yield return null;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading world map...");
		data.chosenWorldMapTemplate = scenarioMapData.worldMapSave.worldMapTemplate;
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data, scenarioMapData));
	}
	private IEnumerator GenerateGrid(MapGenerationData data, ScenarioMapData scenarioMapData) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.XOffset * (data.width / 2f);
		float newY = MapGenerationData.YOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		HexTile[,] map = new HexTile[data.width, data.height];
		List<HexTile> normalHexTiles = new List<HexTile>();

		SaveDataHextile[,] savedMap = scenarioMapData.worldMapSave.GetSaveDataMap();
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				float xPosition = x * MapGenerationData.XOffset;

				float yPosition = y * MapGenerationData.YOffset;
				if (y % 2 == 1) {
					xPosition += MapGenerationData.XOffset / 2;
				}

				SaveDataHextile savedHexTile = savedMap[x, y];
				
				GameObject hex = Object.Instantiate(GridMap.Instance.goHex, GridMap.Instance.transform, true) as GameObject;
				hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
				hex.transform.localScale = new Vector3(MapGenerationData.TileSize, MapGenerationData.TileSize, 0f);
				hex.name = $"{x},{y}";
				HexTile currHex = hex.GetComponent<HexTile>();
				currHex.Initialize();
				savedHexTile.Load(currHex);
				normalHexTiles.Add(currHex);
				map[x, y] = currHex;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, normalHexTiles);
		//Find Neighbours for each hextile
		Parallel.ForEach(normalHexTiles, (hexTile) => {
			hexTile.FindNeighbours(map);
		});
		// for (int i = 0; i < normalHexTiles.Count; i++) {
		// 	HexTile hexTile = normalHexTiles[i];
		// 	hexTile.FindNeighbours(map);
		// }
		yield return null;
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading world map...");
		data.chosenWorldMapTemplate = saveData.worldMapSave.worldMapTemplate;
		WorldSettings.Instance.worldSettingsData.SetWorldType(saveData.worldMapSave.worldType);
		saveData.LoadDate();
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data, saveData));
	}
	private IEnumerator GenerateGrid(MapGenerationData data, SaveDataCurrentProgress saveData) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.XOffset * (data.width / 2f);
		float newY = MapGenerationData.YOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		HexTile[,] map = new HexTile[data.width, data.height];
		List<HexTile> normalHexTiles = new List<HexTile>();

		SaveDataHextile[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				float xPosition = x * MapGenerationData.XOffset;

				float yPosition = y * MapGenerationData.YOffset;
				if (y % 2 == 1) {
					xPosition += MapGenerationData.XOffset / 2;
				}

				SaveDataHextile savedHexTile = savedMap[x, y];
				
				GameObject hex = Object.Instantiate(GridMap.Instance.goHex, GridMap.Instance.transform, true) as GameObject;
				hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
				hex.transform.localScale = new Vector3(MapGenerationData.TileSize, MapGenerationData.TileSize, 0f);
				hex.name = $"{x},{y}";
				HexTile currHex = hex.GetComponent<HexTile>();
				currHex.Initialize();
				savedHexTile.Load(currHex);
				normalHexTiles.Add(currHex);
				map[x, y] = currHex;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, normalHexTiles);
		//Find Neighbours for each hextile
		Parallel.ForEach(normalHexTiles, (hexTile) => {
			hexTile.FindNeighbours(map);
		});
		// for (int i = 0; i < normalHexTiles.Count; i++) {
		// 	HexTile hexTile = normalHexTiles[i];
		// 	hexTile.FindNeighbours(map);
		// }
		yield return null;
	}
	#endregion
	
}