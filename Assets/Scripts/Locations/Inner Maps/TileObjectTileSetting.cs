using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileObjectTileSetting {
    public TileObjectBiomeAssetDictionary biomeAssets;

    public BiomeTileObjectTileSetting GetAsset(BIOMES biome) {
        if (biomeAssets.ContainsKey(biome)) {
            return biomeAssets[biome];
        }
        return biomeAssets[BIOMES.NONE]; //NONE is considered default
    }
}
