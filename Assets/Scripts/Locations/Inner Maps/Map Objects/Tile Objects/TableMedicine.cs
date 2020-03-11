using System.Collections.Generic;

public class TableMedicine : TileObject {
 
    public TableMedicine() {
        Initialize(TILE_OBJECT_TYPE.TABLE_MEDICINE);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableMedicine(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
