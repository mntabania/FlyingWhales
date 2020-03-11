using System.Collections.Generic;

public class ShelfScrolls : TileObject{
    public ShelfScrolls() {
        Initialize(TILE_OBJECT_TYPE.SHELF_SCROLLS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public ShelfScrolls(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
