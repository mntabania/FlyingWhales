using System.Collections.Generic;

public class TableHerbalism : TileObject{
    public TableHerbalism() {
        Initialize(TILE_OBJECT_TYPE.TABLE_HERBALISM);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableHerbalism(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
