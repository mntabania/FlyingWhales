using System.Collections.Generic;

public class WaterBucket : TileObject{
    public WaterBucket() {
        Initialize(TILE_OBJECT_TYPE.WATER_BUCKET);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public WaterBucket(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
        RemoveCommonAdvertisements();
    }

    public override string ToString() {
        return $"Water Bucket {id.ToString()}";
    }
}
