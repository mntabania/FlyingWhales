using Inner_Maps;
using UnityEngine;

public class ElevationIsland : BaseIsland {
    
    public const int MinimumTileRequirement = 100;
    
    public ELEVATION elevation;

    public ElevationIsland(ELEVATION p_elevation) {
        elevation = p_elevation;
        // switch (p_elevation) {
        //     case ELEVATION.PLAIN:
        //         color = Color.green;
        //         break;
        //     case ELEVATION.MOUNTAIN:
        //         color = Color.black;
        //         break;
        //     case ELEVATION.WATER:
        //         color = Color.blue;
        //         break;
        //     // default:
        //     //     throw new ArgumentOutOfRangeException(nameof(p_elevation), p_elevation, null);
        // }
    }
    public override void AddTile(LocationGridTile tile, MapGenerationData mapGenerationData) {
        // tile.parentMap.perlinTilemap.SetColor(tile.localPlace, color);
        base.AddTile(tile, mapGenerationData);
        if (tile.elevationType != elevation) {
            tile.SetElevation(elevation);
        }
    }

    #region Border Tiles
    protected override void AddBorderTile(LocationGridTile p_tile, MapGenerationData mapGenerationData) {
        base.AddBorderTile(p_tile, mapGenerationData);
        if (elevation == ELEVATION.WATER) {
            mapGenerationData.AddOceanBorderTile(p_tile.area, p_tile);
        } else if (elevation == ELEVATION.MOUNTAIN) {
            mapGenerationData.AddCaveBorderTile(p_tile.area, p_tile);
        }
    }
    protected override bool RemoveBorderTile(LocationGridTile p_tile, MapGenerationData mapGenerationData) {
        if (base.RemoveBorderTile(p_tile, mapGenerationData)) {
            if (elevation == ELEVATION.WATER) {
                mapGenerationData.RemoveOceanBorderTile(p_tile.area, p_tile);
            } else if (elevation == ELEVATION.MOUNTAIN) {
                mapGenerationData.RemoveCaveBorderTile(p_tile.area, p_tile);
            }
            return true;
        }
        return false;
    }
    #endregion
}