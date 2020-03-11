using System.Collections.Generic;

public class Grave : TileObject{
    public Grave() {
        Initialize(TILE_OBJECT_TYPE.GRAVE);
    }
    public Grave(SaveDataTileObject data) {
        Initialize(data);
    }
}
