using System.Collections.Generic;

public class Stump : TileObject{
    public Stump() {
        Initialize(TILE_OBJECT_TYPE.STUMP);
    }
    public Stump(SaveDataTileObject data) : base(data) {
        
    }
}
