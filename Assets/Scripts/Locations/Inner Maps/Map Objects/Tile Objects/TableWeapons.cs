using System.Collections.Generic;

public class TableWeapons : TileObject{
    public TableWeapons() {
        Initialize(TILE_OBJECT_TYPE.TABLE_WEAPONS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableWeapons(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
