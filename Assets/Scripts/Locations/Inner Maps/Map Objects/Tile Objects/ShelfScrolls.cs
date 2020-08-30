using System.Collections.Generic;

public class ShelfScrolls : TileObject{
    public ShelfScrolls() {
        Initialize(TILE_OBJECT_TYPE.SHELF_SCROLLS);
    }
    public ShelfScrolls(SaveDataTileObject data) {
        
    }
    
    protected override string GenerateName() { return "Scrolls Shelf"; }
}
