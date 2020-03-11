using System.Collections.Generic;

public class Statue : TileObject{
    public Statue() {
        Initialize(TILE_OBJECT_TYPE.STATUE);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Statue(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
