using System.Collections.Generic;

public class TableConjuring : TileObject{
    public TableConjuring() {
        Initialize(TILE_OBJECT_TYPE.TABLE_CONJURING);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableConjuring(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
