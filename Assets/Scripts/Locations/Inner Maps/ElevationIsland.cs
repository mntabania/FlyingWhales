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
    public override void AddTile(LocationGridTile tile) {
        // tile.parentMap.perlinTilemap.SetColor(tile.localPlace, color);
        base.AddTile(tile);
        if (tile.elevationType != elevation) {
            tile.SetElevation(elevation);
        }
    }

    #region Border Tiles
    protected override void AddBorderTile(LocationGridTile p_tile) {
        // p_tile.parentMap.perlinTilemap.SetColor(p_tile.localPlace, Color.red);    
        base.AddBorderTile(p_tile);
    }
    protected override bool RemoveBorderTile(LocationGridTile p_tile) {
        // p_tile.parentMap.perlinTilemap.SetColor(p_tile.localPlace, color);
        return base.RemoveBorderTile(p_tile);
    }
    #endregion
}