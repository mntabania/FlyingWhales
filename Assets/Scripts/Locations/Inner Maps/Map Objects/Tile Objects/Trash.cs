using System.Collections.Generic;

public class Trash : TileObject{
    public Trash() {
        Initialize(TILE_OBJECT_TYPE.TRASH);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Trash(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
