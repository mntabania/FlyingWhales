using System.Collections.Generic;

public class TableMedicine : TileObject {
 
    public TableMedicine() {
        Initialize(TILE_OBJECT_TYPE.TABLE_MEDICINE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableMedicine(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
