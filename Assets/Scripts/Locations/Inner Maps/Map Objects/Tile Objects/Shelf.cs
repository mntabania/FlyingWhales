using System.Collections.Generic;

public class Shelf : TileObject{
    public Shelf() {
        Initialize(TILE_OBJECT_TYPE.SHELF);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Shelf(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
