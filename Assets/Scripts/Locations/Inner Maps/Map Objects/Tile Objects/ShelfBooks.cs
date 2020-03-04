using System.Collections.Generic;

public class ShelfBooks : TileObject{
    public ShelfBooks() {
        Initialize(TILE_OBJECT_TYPE.SHELF_BOOKS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public ShelfBooks(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }    
}
