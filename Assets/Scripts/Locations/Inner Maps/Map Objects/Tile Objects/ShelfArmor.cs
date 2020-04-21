using System.Collections.Generic;

public class ShelfArmor : TileObject{
    public ShelfArmor() {
        Initialize(TILE_OBJECT_TYPE.SHELF_ARMOR);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public ShelfArmor(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Armor Shelf"; }
}
