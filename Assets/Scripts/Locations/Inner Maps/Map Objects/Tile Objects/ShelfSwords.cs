using System.Collections.Generic;

public class ShelfSwords : TileObject{
    public ShelfSwords() {
        Initialize(TILE_OBJECT_TYPE.SHELF_SWORDS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public ShelfSwords(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
