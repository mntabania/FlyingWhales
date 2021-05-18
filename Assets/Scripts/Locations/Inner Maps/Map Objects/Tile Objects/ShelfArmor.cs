using System.Collections.Generic;

public class ShelfArmor : TileObject{
    public ShelfArmor() {
        Initialize(TILE_OBJECT_TYPE.SHELF_ARMOR);
    }
    public ShelfArmor(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Armor Shelf"; }
}
