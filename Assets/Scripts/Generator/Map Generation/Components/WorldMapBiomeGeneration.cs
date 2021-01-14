using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WorldMapBiomeGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating biomes...");
		yield return MapGenerator.Instance.StartCoroutine(SetBiomePerRegion(data));
		yield return MapGenerator.Instance.StartCoroutine(ElevationBiomeRefinement());
	}
	private IEnumerator SetBiomePerRegion(MapGenerationData data) {
		var choices = WorldSettings.Instance.worldSettingsData.mapSettings.biomes;
		int lastX = 0;
		int lastY = 0;
		int regionIndex = 0;
		foreach (var kvp in data.chosenWorldMapTemplate.regions) {
			for (int i = 0; i < kvp.Value.Length; i++) {
				RegionTemplate regionTemplate = kvp.Value[i];
				BIOMES biome = GetBiomeForRegion(regionIndex, choices);
				choices.Remove(biome);
				SetBiomeForRegionTemplate(regionTemplate, lastX, lastY, biome);
				lastX += regionTemplate.width;
				if (lastX == GridMap.Instance.width) {
					lastX = 0;
					lastY += regionTemplate.height;
				}
				regionIndex++;
			}
		}
		// for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
		// 	Region region = GridMap.Instance.allRegions[i];
		// 	BIOMES biome;
		// 	if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
		// 		biome = BIOMES.GRASSLAND;
		// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
		// 		if (i == 0) {
		// 			biome = BIOMES.FOREST;
		// 		} else if (i == 1) {
		// 			biome = BIOMES.DESERT;
		// 		} else if (i == 2) {
		// 			biome = BIOMES.SNOW;
		// 		} else {
		// 			biome = BIOMES.GRASSLAND;
		// 		}
		// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
		// 		if (i == 0) {
		// 			biome = BIOMES.GRASSLAND;
		// 		} else {
		// 			biome = BIOMES.DESERT;
		// 		} 
		// 	} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
		// 		if (i == 0) {
		// 			biome = BIOMES.FOREST;
		// 		} else {
		// 			biome = BIOMES.SNOW;
		// 		}
		// 	} else {
		// 		biome = CollectionUtilities.GetRandomElement(choices);
		// 	}
		// 	
		// 	for (int j = 0; j < region.tiles.Count; j++) {
		// 		HexTile tile = region.tiles[j];
		// 		Biomes.Instance.SetBiomeForTile(biome, tile);
		// 	}	
		// }
		yield return null;
	}
	private void SetBiomeForRegionTemplate(RegionTemplate p_template, int startingX, int startingY, BIOMES p_biome) {
		int maxX = startingX + p_template.width;
		int maxY = startingY + p_template.height;
		
		for (int x = startingX; x < maxX; x++) {
			for (int y = startingY; y < maxY; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				tile.SetBiome(p_biome);
			}
		}
	}
	private BIOMES GetBiomeForRegion(int p_regionIndex, List<BIOMES> p_biomeChoices) {
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			return BIOMES.GRASSLAND;
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			if (p_regionIndex == 0) {
				return BIOMES.FOREST;
			} else if (p_regionIndex == 1) {
				return BIOMES.DESERT;
			} else if (p_regionIndex == 2) {
				return BIOMES.SNOW;
			} else {
				return BIOMES.GRASSLAND;
			}
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			if (p_regionIndex == 0) {
				return BIOMES.GRASSLAND;
			} else {
				return BIOMES.DESERT;
			} 
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			if (p_regionIndex == 0) {
				return BIOMES.FOREST;
			} else {
				return BIOMES.SNOW;
			}
		} else {
			return CollectionUtilities.GetRandomElement(p_biomeChoices);
		}
	}
	private IEnumerator ElevationBiomeRefinement() {
		int batchCount = 0;
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile tile = GridMap.Instance.normalHexTiles[i];
			if (tile.biomeType == BIOMES.FOREST && tile.elevationType == ELEVATION.PLAIN && GameUtilities.RollChance(75)) {
				tile.SetElevation(ELEVATION.TREES);
			} else if (tile.biomeType == BIOMES.DESERT) {
				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
					if (tile.elevationType == ELEVATION.WATER || tile.elevationType == ELEVATION.MOUNTAIN) {
						tile.SetElevation(GameUtilities.RollChance(65) ? ELEVATION.PLAIN : ELEVATION.TREES);
					}
				} else {
					if (tile.elevationType == ELEVATION.WATER && GameUtilities.RollChance(75)) {
						tile.SetElevation(ELEVATION.PLAIN);	
					} else if (tile.elevationType == ELEVATION.TREES && GameUtilities.RollChance(50)) {
						tile.SetElevation(ELEVATION.PLAIN);	
					}	
				}
			}
			batchCount++;
			if (batchCount >= MapGenerationData.WorldMapElevationRefinementBatches) {
				batchCount = 0;
				yield return null;
			}
		}
	}
}
