using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;
using Object = UnityEngine.Object;

public class AreaGeneration : MapGenerationComponent {

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
								new RegionTemplate(10, 7),
								new RegionTemplate(10, 7),
							}
						},
						{ 1, new [] {
								new RegionTemplate(10, 7),
								new RegionTemplate(10, 7),
							}
						}
					}	
				}
			}
		},
	};

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating World Map...");
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
				worldMapWidth = 9,
				worldMapHeight = 8,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(9, 8),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 1,
				worldMapWidth = 11,
				worldMapHeight = 6,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(11, 6),
						}
					},
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 2,
				worldMapWidth = 13,
				worldMapHeight = 6,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(8, 6),
							new RegionTemplate(5, 6),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 2,
				worldMapWidth = 10,
				worldMapHeight = 12,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(10, 6),
						}
					},
					{
						1, new[] {
							new RegionTemplate(10, 6),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 4,
				worldMapWidth = 16,
				worldMapHeight = 14,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(8, 7),
							new RegionTemplate(8, 7),
						}
					},
					{
						1, new[] {
							new RegionTemplate(8, 7),
							new RegionTemplate(8, 7),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 2,
				worldMapWidth = 15,
				worldMapHeight = 7,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(7, 7),
							new RegionTemplate(8, 7),
						}
					}
				}
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
			chosenTemplate = new WorldMapTemplate() {
				regionCount = 2,
				worldMapWidth = 14,
				worldMapHeight = 12,
				regions = new Dictionary<int, RegionTemplate[]>() {
					{
						0, new[] {
							new RegionTemplate(14, 12),
						}
					}
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
#if DEBUG_LOG
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
#endif
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data));
	}
	private IEnumerator GenerateGrid(MapGenerationData data) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.XOffset * (data.width / 2f);
		float newY = MapGenerationData.YOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		Area[,] map = new Area[data.width, data.height];
		List<Area> areas = new List<Area>();
		int id = 0;

		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				Area area = new Area(id, x, y);
				areas.Add(area);
				map[x, y] = area;
				id++;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, areas);
		//Find Neighbours for each hextile
		Parallel.ForEach(areas, (hexTile) => {
			hexTile.neighbourComponent.FindNeighbours(hexTile, map);
		});
		yield return null;
	}
#endregion

#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading World Map...");
		data.chosenWorldMapTemplate = scenarioMapData.worldMapSave.worldMapTemplate;
#if DEBUG_LOG
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
#endif
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data, scenarioMapData));
	}
	private IEnumerator GenerateGrid(MapGenerationData data, ScenarioMapData scenarioMapData) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.XOffset * (data.width / 2f);
		float newY = MapGenerationData.YOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		Area[,] map = new Area[data.width, data.height];
		List<Area> normalHexTiles = new List<Area>();

		SaveDataArea[,] savedMap = scenarioMapData.worldMapSave.GetSaveDataMap();
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataArea savedHexTile = savedMap[x, y];
				
				Area area = savedHexTile.Load();
				normalHexTiles.Add(area);
				map[x, y] = area;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, normalHexTiles);
		//Find Neighbours for each hextile
		Parallel.ForEach(normalHexTiles, (area) => {
			area.neighbourComponent.FindNeighbours(area, map);
		});
		yield return null;
	}
#endregion
	
#region Saved World
	public override void LoadSavedData(object state) {
		try {
			LoadThreadQueueItem threadItem = state as LoadThreadQueueItem;
			MapGenerationData mapData = threadItem.mapData;
			SaveDataCurrentProgress saveData = threadItem.saveData;

			//mapData.chosenWorldMapTemplate = saveData.worldMapSave.worldMapTemplate;
			//WorldSettings.Instance.worldSettingsData.SetWorldType(saveData.worldMapSave.worldType);
			//saveData.LoadDate();

			GridMap.Instance.SetupInitialData(mapData.width, mapData.height);
			//float newX = MapGenerationData.XOffset * (mapData.width / 2f);
			//float newY = MapGenerationData.YOffset * (mapData.height / 2f);
			//GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
			Area[,] map = new Area[mapData.width, mapData.height];
			List<Area> normalHexTiles = new List<Area>();

			SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();

			//int batchCount = 0;
			for (int x = 0; x < mapData.width; x++) {
				for (int y = 0; y < mapData.height; y++) {
					SaveDataArea savedHexTile = savedMap[x, y];

					Area area = savedHexTile.Load();
					normalHexTiles.Add(area);
					map[x, y] = area;

					//batchCount++;
					//if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					//	batchCount = 0;
					//	yield return null;
					//}
				}
			}

			GridMap.Instance.SetMap(map, normalHexTiles);
			for (int i = 0; i < normalHexTiles.Count; i++) {
				Area area = normalHexTiles[i];
				area.neighbourComponent.FindNeighbours(area, map);
			}

			////Find Neighbours for each hextile
			//Parallel.ForEach(normalHexTiles, (area) => {
			//	area.neighbourComponent.FindNeighbours(area, map);
			//});
			threadItem.isDone = true;
		} catch (Exception e) {
			Debug.LogError(e.Message + "\n" + e.StackTrace);
		}
		
	}
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading world map...");
		data.chosenWorldMapTemplate = saveData.worldMapSave.worldMapTemplate;
		WorldSettings.Instance.worldSettingsData.SetWorldType(saveData.worldMapSave.worldType);
		saveData.LoadDate();
#if DEBUG_LOG
		Debug.Log($"Width: {data.width.ToString()} Height: {data.height.ToString()} Region Count: {data.regionCount.ToString()}");
#endif
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data, saveData));
	}
	private IEnumerator GenerateGrid(MapGenerationData data, SaveDataCurrentProgress saveData) {
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float newX = MapGenerationData.XOffset * (data.width / 2f);
		float newY = MapGenerationData.YOffset * (data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(-newX, -newY);
		Area[,] map = new Area[data.width, data.height];
		List<Area> normalHexTiles = new List<Area>();

		SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataArea savedHexTile = savedMap[x, y];
				
				Area area = savedHexTile.Load();
				normalHexTiles.Add(area);
				map[x, y] = area;

				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
					batchCount = 0;
					yield return null;    
				}
			}
		}
		
		GridMap.Instance.SetMap(map, normalHexTiles);
		//Find Neighbours for each hextile
		Parallel.ForEach(normalHexTiles, (area) => {
			area.neighbourComponent.FindNeighbours(area, map);
		});
		yield return null;
	}
#endregion
	
}