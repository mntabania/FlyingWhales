using System.Collections.Generic;

public class TableConjuring : TileObject{
    public TableConjuring() {
        Initialize(TILE_OBJECT_TYPE.TABLE_CONJURING);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableConjuring(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Conjuring Table"; }
}
