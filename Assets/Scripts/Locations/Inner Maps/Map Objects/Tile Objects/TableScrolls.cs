using System.Collections.Generic;

public class TableScrolls : TileObject{
    public TableScrolls() {
        Initialize(TILE_OBJECT_TYPE.TABLE_SCROLLS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableScrolls(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
