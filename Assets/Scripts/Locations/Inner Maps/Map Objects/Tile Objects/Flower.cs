using System.Collections.Generic;

public class Flower : TileObject{
    public Flower() {
        Initialize(TILE_OBJECT_TYPE.FLOWER, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Flower(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
