using System.Collections.Generic;

public class ShelfArmor : TileObject{
    public ShelfArmor() {
        Initialize(TILE_OBJECT_TYPE.SHELF_ARMOR);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public ShelfArmor(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
