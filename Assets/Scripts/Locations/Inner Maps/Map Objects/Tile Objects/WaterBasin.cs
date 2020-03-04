using System.Collections.Generic;

public class WaterBasin : TileObject{
    
    public WaterBasin() {
        Initialize(TILE_OBJECT_TYPE.WATER_BASIN);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public WaterBasin(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
