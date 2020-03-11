using System.Collections.Generic;

public class ShelfSwords : TileObject{
    public ShelfSwords() {
        Initialize(TILE_OBJECT_TYPE.SHELF_SWORDS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public ShelfSwords(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
