using System.Collections.Generic;

public class TableScrolls : TileObject{
    public TableScrolls() {
        Initialize(TILE_OBJECT_TYPE.TABLE_SCROLLS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableScrolls(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
