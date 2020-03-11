using System.Collections.Generic;

public class Rug : TileObject{
    public Rug() { 
        Initialize(TILE_OBJECT_TYPE.RUG);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Rug(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

}
