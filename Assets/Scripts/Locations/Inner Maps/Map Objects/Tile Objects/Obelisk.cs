using System.Collections.Generic;

public class Obelisk : TileObject{
    public Obelisk() {
        Initialize(TILE_OBJECT_TYPE.OBELISK);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Obelisk(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}