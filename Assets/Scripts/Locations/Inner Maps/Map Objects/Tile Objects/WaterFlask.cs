using System.Collections.Generic;

public class WaterFlask : TileObject{
    public WaterFlask() {
        Initialize(TILE_OBJECT_TYPE.WATER_FLASK);
    }
    public WaterFlask(SaveDataTileObject data) : base(data) {
        
    }

    public override string ToString() {
        return $"Water Flask {id.ToString()}";
    }
}
