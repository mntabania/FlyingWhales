using System.Collections.Generic;

public class Fireplace : TileObject{
    public Fireplace() {
        Initialize(TILE_OBJECT_TYPE.FIREPLACE);
    }
    public Fireplace(SaveDataTileObject data) {
        Initialize(data);
    }
}
