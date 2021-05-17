using System.Collections.Generic;

public class ShelfSwords : TileObject{
    public ShelfSwords() {
        Initialize(TILE_OBJECT_TYPE.SHELF_SWORDS);
    }
    public ShelfSwords(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Swords"; }
}
