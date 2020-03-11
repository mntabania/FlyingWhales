using System.Collections.Generic;

public class WaterBasin : TileObject{
    
    public WaterBasin() {
        Initialize(TILE_OBJECT_TYPE.WATER_BASIN);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public WaterBasin(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
