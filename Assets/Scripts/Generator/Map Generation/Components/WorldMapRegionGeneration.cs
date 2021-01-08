using System;
using System.Collections;
using System.Collections.Generic;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;

public class WorldMapRegionGeneration : MapGenerationComponent {

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating regions...");
		WorldMapTemplate chosenTemplate = data.chosenWorldMapTemplate;
		yield return MapGenerator.Instance.StartCoroutine(DivideToRegions(chosenTemplate, data));
	}
	private IEnumerator DivideToRegions(WorldMapTemplate mapTemplate, MapGenerationData data) {
		// int lastX = 0;
		// int lastY = 0;
		// Region[] allRegions = new Region[data.regionCount];
		// int regionIndex = 0;
		// foreach (var mapTemplateRegion in mapTemplate.regions) {
		// 	for (int i = 0; i < mapTemplateRegion.Value.Length; i++) {
		// 		RegionTemplate regionTemplate = mapTemplateRegion.Value[i];
		// 		Region region = CreateNewRegionFromTemplate(regionTemplate, lastX, lastY);
		//
		// 		lastX += regionTemplate.width;
		// 		if (lastX == GridMap.Instance.width) {
		// 			lastX = 0;
		// 			lastY += regionTemplate.height;
		// 		}
		// 		allRegions[regionIndex] = region;
		// 		regionIndex++;
		//
		// 	}
		// }
		
		
		int centerX = mapTemplate.worldMapWidth / 2;
		int centerY = mapTemplate.worldMapHeight / 2;
		HexTile center = GridMap.Instance.map[centerX, centerY];
		Region region = new Region(center);
		Region[] allRegions = { region };
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile hexTile = GridMap.Instance.normalHexTiles[i];
			region.AddTile(hexTile);
		}
		
		GridMap.Instance.SetRegions(allRegions);
		// string summary = "Region Generation Summary: ";
		// for (int i = 0; i < allRegions.Length; i++) {
		// 	Region region = allRegions[i];
  //           region.PopulateNeighbours();
		// 	summary += $"\n{region.name} - {region.tiles.Count.ToString()}";
		// }
		// Debug.Log(summary);
		
		yield return null;
	}
	private Region CreateNewRegionFromTemplate(RegionTemplate template, int startingX, int startingY) {
		int maxX = startingX + template.width;
		int maxY = startingY + template.height;
		
		int centerX = startingX + (template.width / 2);
		int centerY = startingY + (template.height / 2);
		HexTile center = GridMap.Instance.map[centerX, centerY];
		
		Region region = new Region(center);
		for (int x = startingX; x < maxX; x++) {
			for (int y = startingY; y < maxY; y++) {
				try {
					HexTile tile = GridMap.Instance.map[x, y];
					region.AddTile(tile);
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}
				
			}
		}

		return region;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading regions...");
		yield return MapGenerator.Instance.StartCoroutine(LoadRegions(saveData));
	}
	private IEnumerator LoadRegions(SaveDataCurrentProgress saveData) {
		int lastX = 0;
		int lastY = 0;
		Region[] allRegions = new Region[saveData.worldMapSave.regionSaves.Count];

		for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
			SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
			Region region = CreateNewRegionFromSave(saveDataRegion, lastX, lastY, saveData.worldMapSave.worldMapTemplate.worldMapWidth, saveData.worldMapSave.worldMapTemplate.worldMapHeight);
			// lastX += saveDataRegion.regionTemplate.width;
			// if (lastX == GridMap.Instance.width) {
			// 	lastX = 0;
			// 	lastY += saveDataRegion.regionTemplate.height;
			// }
			allRegions[i] = region;
		}
		GridMap.Instance.SetRegions(allRegions);
		string summary = "Region Generation Summary: ";
		for (int i = 0; i < allRegions.Length; i++) {
			Region region = allRegions[i];
			summary += $"\n{region.name} - {region.tiles.Count.ToString()}";
		}
		Debug.Log(summary);
		
		yield return null;
	}
	private Region CreateNewRegionFromSave(SaveDataRegion saveDataRegion, int startingX, int startingY, int maxX, int maxY) {
		Region region = new Region(saveDataRegion);
		for (int x = startingX; x < maxX; x++) {
			for (int y = startingY; y < maxY; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				region.AddTile(tile);
			}
		}

		return region;
	}
	#endregion
}

[Serializable]
public struct WorldMapTemplate {
	public int regionCount;
	public int worldMapWidth;
	public int worldMapHeight;
	public Dictionary<int, RegionTemplate[]> regions; //key is row
}
[Serializable]
public struct RegionTemplate {
	public readonly int width;
	public readonly int height;

	public RegionTemplate(int width, int height) {
		this.width = width;
		this.height = height;
	}
}
