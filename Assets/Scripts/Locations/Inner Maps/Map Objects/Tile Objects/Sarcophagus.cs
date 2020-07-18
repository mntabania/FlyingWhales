using System.Collections.Generic;

public class Sarcophagus : TileObject{
    public Sarcophagus() {
        Initialize(TILE_OBJECT_TYPE.SARCOPHAGUS);
    }
    public Sarcophagus(SaveDataTileObject data) {
        Initialize(data);
    }
}