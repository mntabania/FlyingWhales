using System.Collections.Generic;

public class DemonAltar : TileObject{
    public DemonAltar() {
        Initialize(TILE_OBJECT_TYPE.DEMON_ALTAR);
    }
    public DemonAltar(SaveDataTileObject data) {
        Initialize(data);
    }
}