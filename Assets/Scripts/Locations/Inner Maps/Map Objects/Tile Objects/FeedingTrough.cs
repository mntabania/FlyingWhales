using System.Collections.Generic;

public class FeedingTrough : TileObject{
    public FeedingTrough() {
        Initialize(TILE_OBJECT_TYPE.FEEDING_TROUGH);
    }
    public FeedingTrough(SaveDataTileObject data) {
        Initialize(data);
    }
}