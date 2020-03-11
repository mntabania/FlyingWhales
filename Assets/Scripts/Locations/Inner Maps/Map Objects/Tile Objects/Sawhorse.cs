using System.Collections.Generic;

public class Sawhorse : TileObject{
    public Sawhorse() {
        Initialize(TILE_OBJECT_TYPE.SAWHORSE);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Sawhorse(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}