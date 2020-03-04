using System.Collections.Generic;

public class TableAlchemy : TileObject{
    public TableAlchemy() {
        Initialize(TILE_OBJECT_TYPE.TABLE_ALCHEMY);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableAlchemy(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
