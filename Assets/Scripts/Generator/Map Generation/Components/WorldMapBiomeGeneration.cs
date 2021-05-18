using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class WorldMapBiomeGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Biomes...");
		// yield return MapGenerator.Instance.StartCoroutine(SetBiomePerRegion(data));
		// yield return MapGenerator.Instance.StartCoroutine(ElevationBiomeRefinement());
		yield return null;
	}
	// private IEnumerator SetBiomePerRegion(MapGenerationData data) {
	// 	var choices = new List<BIOMES>(WorldSettings.Instance.worldSettingsData.mapSettings.biomes);
	// 	int lastX = 0;
	// 	int lastY = 0;
	// 	int regionIndex = 0;
	// 	Region region = GridMap.Instance.allRegions.First();
	// 	foreach (var kvp in data.chosenWorldMapTemplate.regions) {
	// 		for (int i = 0; i < kvp.Value.Length; i++) {
	// 			RegionTemplate regionTemplate = kvp.Value[i];
	// 			BIOMES biome = GetBiomeForRegion(regionIndex, choices);
	// 			choices.Remove(biome);
	// 			if (choices.Count <= 0) {
	// 				choices.AddRange(WorldSettings.Instance.worldSettingsData.mapSettings.biomes);
	// 			}
	// 			SetBiomeForRegionDivisionTemplate(regionTemplate, lastX, lastY, biome, region);
	// 			lastX += regionTemplate.width;
	// 			if (lastX == GridMap.Instance.width) {
	// 				lastX = 0;
	// 				lastY += regionTemplate.height;
	// 			}
	// 			regionIndex++;
	// 		}
	// 	}
	// 	yield return null;
	// }
	// private void SetBiomeForRegionDivisionTemplate(RegionTemplate p_template, int startingX, int startingY, BIOMES p_biome, Region p_region) {
	// 	int maxX = startingX + p_template.width;
	// 	int maxY = startingY + p_template.height;
	// 	BiomeDivision biomeDivision = new BiomeDivision(p_biome);
	// 	for (int x = startingX; x < maxX; x++) {
	// 		for (int y = startingY; y < maxY; y++) {
	// 			Area area = GridMap.Instance.map[x, y];
	// 			// biomeDivision.AddTile(area);
	// 		}
	// 	}
	// 	p_region.biomeDivisionComponent.AddBiomeDivision(biomeDivision);
	// }
	// private BIOMES GetBiomeForRegion(int p_regionIndex, List<BIOMES> p_biomeChoices) {
	// 	if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
	// 		return BIOMES.GRASSLAND;
	// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
	// 		if (p_regionIndex == 0) {
	// 			return BIOMES.GRASSLAND;
	// 		} else {
	// 			return BIOMES.DESERT;
	// 		} 
	// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
	// 		if (p_regionIndex == 0) {
	// 			return BIOMES.FOREST;
	// 		} else {
	// 			return BIOMES.SNOW;
	// 		}
	// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
	// 		if (p_regionIndex == 0) {
	// 			return BIOMES.FOREST;
	// 		} else if (p_regionIndex == 1) {
	// 			return BIOMES.DESERT;
	// 		} else if (p_regionIndex == 2) {
	// 			return BIOMES.SNOW;
	// 		} else {
	// 			return BIOMES.GRASSLAND;
	// 		}
	// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
	// 		if (p_regionIndex == 0) {
	// 			return BIOMES.FOREST;
	// 		} else {
	// 			return BIOMES.SNOW;
	// 		}
	// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
	// 		return BIOMES.FOREST;
	// 	} else {
	// 		return CollectionUtilities.GetRandomElement(p_biomeChoices);
	// 	}
	// }
	// private IEnumerator ElevationBiomeRefinement() {
	// 	int batchCount = 0;
	// 	for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
	// 		Area area = GridMap.Instance.allAreas[i];
	// 		if (area.biomeType == BIOMES.FOREST && area.elevationType == ELEVATION.PLAIN && GameUtilities.RollChance(75)) {
	// 			area.SetElevation(ELEVATION.TREES);
	// 		} else if (area.biomeType == BIOMES.DESERT) {
	// 			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
	// 				if (area.elevationType == ELEVATION.WATER || area.elevationType == ELEVATION.MOUNTAIN) {
	// 					area.SetElevation(GameUtilities.RollChance(65) ? ELEVATION.PLAIN : ELEVATION.TREES);
	// 				}
	// 			} else {
	// 				if (area.elevationType == ELEVATION.WATER && GameUtilities.RollChance(75)) {
	// 					area.SetElevation(ELEVATION.PLAIN);	
	// 				} else if (area.elevationType == ELEVATION.TREES && GameUtilities.RollChance(50)) {
	// 					area.SetElevation(ELEVATION.PLAIN);	
	// 				}	
	// 			}
	// 		}
	// 		batchCount++;
	// 		if (batchCount >= MapGenerationData.WorldMapElevationRefinementBatches) {
	// 			batchCount = 0;
	// 			yield return null;
	// 		}
	// 	}
	// }
}
