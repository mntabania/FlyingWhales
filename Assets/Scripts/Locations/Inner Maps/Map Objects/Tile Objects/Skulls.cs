using System.Collections.Generic;

public class Skulls : TileObject{
    public Skulls() {
        Initialize(TILE_OBJECT_TYPE.SKULLS);
    }
    public Skulls(SaveDataTileObject data) {
        Initialize(data);
    }
}