using System.Collections.Generic;

public class Manacles : TileObject{
    public Manacles() {
        Initialize(TILE_OBJECT_TYPE.MANACLES);
    }
    public Manacles(SaveDataTileObject data) {
        Initialize(data);
    }
}