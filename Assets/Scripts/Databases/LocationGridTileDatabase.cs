using System;
using System.Collections.Generic;
using Inner_Maps;

public class LocationGridTileDatabase {
    
    public Dictionary<string, LocationGridTile> tileByGUID { get; }
    public List<LocationGridTile> LocationGridTiles { get; }
    
    public LocationGridTileDatabase() {
        tileByGUID = new Dictionary<string, LocationGridTile>();
        LocationGridTiles = new List<LocationGridTile>();
    }

    public void RegisterTile(LocationGridTile tile) {
        tileByGUID.Add(tile.persistentID, tile);
        LocationGridTiles.Add(tile);
    }
    public LocationGridTile GetTileByPersistentID(string id) {
        if (tileByGUID.ContainsKey(id)) {
            return tileByGUID[id];
        }
        throw new Exception($"There is no Location Grid Tile with id {id}");
    }
    
}
