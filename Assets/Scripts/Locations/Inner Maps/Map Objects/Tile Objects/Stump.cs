using System.Collections.Generic;

public class Stump : TileObject{
    public Stump() {
        Initialize(TILE_OBJECT_TYPE.STUMP);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Stump(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
