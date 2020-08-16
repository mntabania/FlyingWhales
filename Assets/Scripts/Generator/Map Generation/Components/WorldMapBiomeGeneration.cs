using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WorldMapBiomeGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating biomes...");
		yield return MapGenerator.Instance.StartCoroutine(SetBiomePerRegion());
		yield return MapGenerator.Instance.StartCoroutine(ElevationBiomeRefinement());
	}
	private IEnumerator SetBiomePerRegion() {
		var choices = WorldSettings.Instance.worldSettingsData.biomes;
		 
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			BIOMES biome;
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
				if (i == 0) {
					biome = BIOMES.FOREST;
				} else if (i == 1) {
					biome = BIOMES.DESERT;
				} else if (i == 2) {
					biome = BIOMES.SNOW;
				} else {
					biome = BIOMES.GRASSLAND;
				}
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
				if (i == 0) {
					biome = BIOMES.GRASSLAND;
				} else {
					biome = BIOMES.DESERT;
				} 
			} else {
				biome = CollectionUtilities.GetRandomElement(choices);
			}
			
			for (int j = 0; j < region.tiles.Count; j++) {
				HexTile tile = region.tiles[j];
				Biomes.Instance.SetBiomeForTile(biome, tile);
			}	
		}
		yield return null;
	}
	private IEnumerator ElevationBiomeRefinement() {
		int batchCount = 0;
		for (int i = 0; i < GridMap.Instance.allTiles.Count; i++) {
			HexTile tile = GridMap.Instance.allTiles[i];
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
