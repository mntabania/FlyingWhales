using System.Collections.Generic;

public class WaterFlask : TileObject{
    public WaterFlask() {
        Initialize(TILE_OBJECT_TYPE.WATER_FLASK);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public WaterFlask(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

    public override string ToString() {
        return $"Water Flask {id.ToString()}";
    }
}
