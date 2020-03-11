using System.Collections.Generic;

public class ShelfScrolls : TileObject{
    public ShelfScrolls() {
        Initialize(TILE_OBJECT_TYPE.SHELF_SCROLLS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public ShelfScrolls(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
