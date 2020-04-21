using System.Collections.Generic;

public class TableArmor : TileObject{
    public TableArmor() {
        Initialize(TILE_OBJECT_TYPE.TABLE_ARMOR);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableArmor(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Armor Table"; }
}
