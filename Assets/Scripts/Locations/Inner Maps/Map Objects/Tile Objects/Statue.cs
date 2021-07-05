using System.Collections.Generic;

public class Statue : TileObject{
    public Statue() {
        Initialize(TILE_OBJECT_TYPE.STATUE);
    }
    public Statue(SaveDataTileObject data) : base(data) {
        
    }
}
