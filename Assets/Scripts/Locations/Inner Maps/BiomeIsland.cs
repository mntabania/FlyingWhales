using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class BiomeIsland : BaseIsland {
    public const int MinimumTilesInIsland = 100;
    public readonly BIOMES biome;
    public BiomeIsland(BIOMES biome) : base() {
        this.biome = biome;
    }
    public override void AddTile(LocationGridTile tile) {
        base.AddTile(tile);
        // tile.parentMap.perlinTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.grassTile);
        // // tile.parentMap.perlinTilemap.SetColor(tile.localPlace, color);
        if (tile.biomeType != biome) {
            tile.SetIndividualBiomeType(biome);
        }
    }
}