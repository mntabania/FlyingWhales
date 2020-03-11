using System.Collections.Generic;

public class TableHerbalism : TileObject{
    public TableHerbalism() {
        Initialize(TILE_OBJECT_TYPE.TABLE_HERBALISM);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableHerbalism(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
