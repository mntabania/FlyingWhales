using System.Collections.Generic;

public class TableWeapons : TileObject{
    public TableWeapons() {
        Initialize(TILE_OBJECT_TYPE.TABLE_WEAPONS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableWeapons(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
