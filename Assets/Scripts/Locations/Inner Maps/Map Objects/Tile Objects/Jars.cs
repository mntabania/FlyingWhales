using System.Collections.Generic;

public class Jars : TileObject{
    public Jars() {
        Initialize(TILE_OBJECT_TYPE.JARS);
    }
    public Jars(SaveDataTileObject data) {
        Initialize(data);
    }
}