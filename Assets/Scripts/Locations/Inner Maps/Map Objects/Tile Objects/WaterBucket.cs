using System.Collections.Generic;

public class WaterBucket : TileObject{
    public WaterBucket() {
        Initialize(TILE_OBJECT_TYPE.WATER_BUCKET);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public WaterBucket(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

    public override string ToString() {
        return $"Water Bucket {id.ToString()}";
    }
}
