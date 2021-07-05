using System.Collections.Generic;

public class TableScrolls : TileObject{
    public TableScrolls() {
        Initialize(TILE_OBJECT_TYPE.TABLE_SCROLLS);
    }
    public TableScrolls(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Scrolls"; }
}
