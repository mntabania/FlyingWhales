using System.Collections.Generic;

public class Plant : TileObject{
    public Plant() {
        Initialize(TILE_OBJECT_TYPE.PLANT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Plant(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
