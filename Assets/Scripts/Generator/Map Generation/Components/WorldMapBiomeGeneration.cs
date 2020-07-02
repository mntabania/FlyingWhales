using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WorldMapBiomeGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating biomes...");
		yield return MapGenerator.Instance.StartCoroutine(SetBiomePerRegion());
		yield return MapGenerator.Instance.StartCoroutine(ElevationBiomeRefinement());
	}

	private IEnumerator SetBiomePerRegion() {
		var choices = WorldConfigManager.Instance.isDemoWorld ? 
			new List<BIOMES>(){ BIOMES.GRASSLAND } : 
			WorldSettings.Instance.worldSettingsData.biomes;
		 
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			BIOMES randomBiome = CollectionUtilities.GetRandomElement(choices);
			for (int j = 0; j < region.tiles.Count; j++) {
				HexTile tile = region.tiles[j];
				Biomes.Instance.SetBiomeForTile(randomBiome, tile);
			}
		}
		yield return null;
	}
	private bool HasRegionWithBiome(BIOMES biome) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.coreTile.biomeType == biome) {
				return true;
			}
		}
		return false;
	}

	private IEnumerator ElevationBiomeRefinement() {
		int batchCount = 0;
		for (int i = 0; i < GridMap.Instance.allTiles.Count; i++) {
			HexTile tile = GridMap.Instance.allTiles[i];
			if (tile.biomeType == BIOMES.FOREST && tile.elevationType == ELEVATION.PLAIN && GameUtilities.RollChance(75)) {
				tile.SetElevation(ELEVATION.TREES);
			} else if (tile.biomeType == BIOMES.DESERT) {
				if (tile.elevationType == ELEVATION.WATER && GameUtilities.RollChance(75)) {
					tile.SetElevation(ELEVATION.PLAIN);	
				} else if (tile.elevationType == ELEVATION.TREES && GameUtilities.RollChance(50)) {
					tile.SetElevation(ELEVATION.PLAIN);	
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
