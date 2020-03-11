using System.Collections.Generic;

public class TempleAltar : TileObject{
    public TempleAltar() {
        Initialize(TILE_OBJECT_TYPE.TEMPLE_ALTAR);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TempleAltar(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
