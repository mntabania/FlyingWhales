using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;

public class WorldMapRegionGeneration : MapGenerationComponent {

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Regions...");
		WorldMapTemplate chosenTemplate = data.chosenWorldMapTemplate;
		yield return MapGenerator.Instance.StartCoroutine(DivideToRegions(chosenTemplate, data));
		CreateBiomeDivisions();
		yield return null;
	}
	private void CreateBiomeDivisions() {
		BIOMES[] biomes = UtilityScripts.CollectionUtilities.GetEnumValues<BIOMES>();
		for (int i = 0; i < biomes.Length; i++) {
			BIOMES biome = biomes[i];
			BiomeDivision biomeDivision = new BiomeDivision(biome);
			GridMap.Instance.mainRegion.biomeDivisionComponent.AddBiomeDivision(biomeDivision);
		}
	}
	private IEnumerator DivideToRegions(WorldMapTemplate mapTemplate, MapGenerationData data) {
		int centerX = mapTemplate.worldMapWidth / 2;
		int centerY = mapTemplate.worldMapHeight / 2;
		Area center = GridMap.Instance.map[centerX, centerY];
		string regionName = string.Empty;
		if (WorldSettings.Instance.worldSettingsData.IsScenarioMap() && WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial) {
			regionName = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(WorldSettings.Instance.worldSettingsData.worldType.ToString());
		}
		Region region = new Region(center, regionName);
		Region[] allRegions = { region };
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
			Area hexTile = GridMap.Instance.allAreas[i];
			region.AddTile(hexTile);
		}
		
		GridMap.Instance.SetRegions(allRegions);
		yield return null;
	}
	// private BiomeDivision CreateNewRegionDivisionFromTemplate(RegionTemplate template, int startingX, int startingY) {
	// 	int maxX = startingX + template.width;
	// 	int maxY = startingY + template.height;
	// 	
	// 	int centerX = startingX + (template.width / 2);
	// 	int centerY = startingY + (template.height / 2);
	// 	Area center = GridMap.Instance.map[centerX, centerY];
	//
	// 	BiomeDivision biomeDivision = new BiomeDivision(center.biomeType);
	// 	for (int x = startingX; x < maxX; x++) {
	// 		for (int y = startingY; y < maxY; y++) {
	// 			Area tile = GridMap.Instance.map[x, y];
	// 			biomeDivision.AddTile(tile);
	// 		}
	// 	}
	//
	// 	return biomeDivision;
	// }
    // private void PopulateRegionDivisionHexTilesFromTemplate(BiomeDivision biomeDivision, RegionTemplate template, int startingX, int startingY) {
    //     int maxX = startingX + template.width;
    //     int maxY = startingY + template.height;
    //
    //     int centerX = startingX + (template.width / 2);
    //     int centerY = startingY + (template.height / 2);
    //
    //     for (int x = startingX; x < maxX; x++) {
    //         for (int y = startingY; y < maxY; y++) {
	   //          Area tile = GridMap.Instance.map[x, y];
    //             biomeDivision.AddTile(tile);
    //         }
    //     }
    // }
    #endregion

    #region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
		// LoadScenarioRegionDivisions(data.chosenWorldMapTemplate, GridMap.Instance.allRegions.First());
	}
	// private void LoadScenarioRegionDivisions(WorldMapTemplate p_template, Region p_region) {
	// 	int lastX = 0;
	// 	int lastY = 0;
	// 	foreach (var mapTemplateRegion in p_template.regions) {
	// 		for (int i = 0; i < mapTemplateRegion.Value.Length; i++) {
	// 			RegionTemplate regionTemplate = mapTemplateRegion.Value[i];
	// 			BiomeDivision division = CreateNewRegionDivisionFromTemplate(regionTemplate, lastX, lastY);
	// 			lastX += regionTemplate.width;
	// 			if (lastX == GridMap.Instance.width) {
	// 				lastX = 0;
	// 				lastY += regionTemplate.height;
	// 			}
	// 			p_region.biomeDivisionComponent.AddBiomeDivision(division);
	// 		}
	// 	}
	// }
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Regions...");
		yield return MapGenerator.Instance.StartCoroutine(LoadRegions(saveData));
		// LoadSavedRegionDivisions(data.chosenWorldMapTemplate, GridMap.Instance.allRegions.First());
		// CreateBiomeDivisions();
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
#if DEBUG_LOG
		string summary = "Region Generation Summary: ";
		for (int i = 0; i < allRegions.Length; i++) {
			Region region = allRegions[i];
			summary += $"\n{region.name} - {region.areas.Count.ToString()}";
		}
		Debug.Log(summary);
#endif
		
		yield return null;
	}
	private Region CreateNewRegionFromSave(SaveDataRegion saveDataRegion, int startingX, int startingY, int maxX, int maxY) {
		Region region = new Region(saveDataRegion);
		for (int x = startingX; x < maxX; x++) {
			for (int y = startingY; y < maxY; y++) {
				Area tile = GridMap.Instance.map[x, y];
				region.AddTile(tile);
			}
		}

		return region;
	}
    // private void LoadSavedRegionDivisions(WorldMapTemplate p_template, Region p_region) {
    //     int lastX = 0;
    //     int lastY = 0;
    //     int regionIndex = 0;
    //     foreach (var mapTemplateRegion in p_template.regions) {
    //         for (int i = 0; i < mapTemplateRegion.Value.Length; i++) {
    //             RegionTemplate regionTemplate = mapTemplateRegion.Value[i];
    //             BiomeDivision division = p_region.biomeDivisionComponent.divisions[regionIndex];
    //             PopulateRegionDivisionHexTilesFromTemplate(division, regionTemplate, lastX, lastY);
    //             lastX += regionTemplate.width;
    //             if (lastX == GridMap.Instance.width) {
    //                 lastX = 0;
    //                 lastY += regionTemplate.height;
    //             }
    //             regionIndex++;
    //         }
    //     }
    // }
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
