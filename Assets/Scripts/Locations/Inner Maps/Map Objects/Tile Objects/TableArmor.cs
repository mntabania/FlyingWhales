using System.Collections.Generic;

public class TableArmor : TileObject{
    public TableArmor() {
        Initialize(TILE_OBJECT_TYPE.TABLE_ARMOR);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableArmor(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
