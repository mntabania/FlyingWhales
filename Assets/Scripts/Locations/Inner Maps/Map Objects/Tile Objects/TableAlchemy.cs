using System.Collections.Generic;

public class TableAlchemy : TileObject{
    public TableAlchemy() {
        Initialize(TILE_OBJECT_TYPE.TABLE_ALCHEMY);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableAlchemy(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Alchemy Table"; }
}
