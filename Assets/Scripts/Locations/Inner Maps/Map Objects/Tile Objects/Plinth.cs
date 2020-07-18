using System.Collections.Generic;

public class Plinth : TileObject{
    public Plinth() {
        Initialize(TILE_OBJECT_TYPE.PLINTH);
    }
    public Plinth(SaveDataTileObject data) {
        Initialize(data);
    }
}