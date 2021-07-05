using System.Collections.Generic;

public class WaterBasin : TileObject{
    
    public WaterBasin() {
        Initialize(TILE_OBJECT_TYPE.WATER_BASIN);
    }
    public WaterBasin(SaveDataTileObject data) : base(data) {
        
    }
}
