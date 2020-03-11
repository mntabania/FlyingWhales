using System.Collections.Generic;

public class Torch : TileObject{
    public Torch() {
        Initialize(TILE_OBJECT_TYPE.TORCH);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Torch(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
