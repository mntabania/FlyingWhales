using System.Collections.Generic;

public class Obelisk : TileObject{
    public Obelisk() {
        Initialize(TILE_OBJECT_TYPE.OBELISK);
    }
    public Obelisk(SaveDataTileObject data) : base(data) {
        
    }
}