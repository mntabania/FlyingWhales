using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WorldMapBiomeGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		int batchCount = 0;
		List<BIOMES> choices = new List<BIOMES>(){ BIOMES.DESERT, BIOMES.GRASSLAND, BIOMES.FOREST, BIOMES.SNOW };
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			BIOMES randomBiome = CollectionUtilities.GetRandomElement(choices);
			for (int j = 0; j < region.tiles.Count; j++) {
				HexTile tile = region.tiles[j];
				Biomes.Instance.SetBiomeForTile(randomBiome, tile);
			}
			
			batchCount++;
			if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
				batchCount = 0;
				yield return null;    
			}
		}
	}
}
