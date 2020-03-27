using System.Collections.Generic;

public class Candles : TileObject{
    public Candles() {
        Initialize(TILE_OBJECT_TYPE.CANDLES);
    }
    public Candles(SaveDataTileObject data) {
        Initialize(data);
    }
}