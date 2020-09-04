using System.Collections.Generic;

public class Barrel : TileObject{
    public Barrel() {
        Initialize(TILE_OBJECT_TYPE.BARREL);
    }
    public Barrel(SaveDataTileObject data) : base(data) { }
}
