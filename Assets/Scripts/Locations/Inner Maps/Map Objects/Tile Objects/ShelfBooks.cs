using System.Collections.Generic;

public class ShelfBooks : TileObject{
    public ShelfBooks() {
        Initialize(TILE_OBJECT_TYPE.SHELF_BOOKS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public ShelfBooks(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
