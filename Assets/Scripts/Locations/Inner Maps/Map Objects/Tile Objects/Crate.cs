using System.Collections.Generic;

public class Crate : TileObject{
    public Crate() {
        Initialize(TILE_OBJECT_TYPE.CRATE);
    }
    public Crate(SaveDataTileObject data) : base(data) {
        
    }
}
