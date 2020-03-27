using System.Collections.Generic;

public class TortureTable : TileObject{
    public TortureTable() {
        Initialize(TILE_OBJECT_TYPE.TORTURE_TABLE);
    }
    public TortureTable(SaveDataTileObject data) {
        Initialize(data);
    }
}