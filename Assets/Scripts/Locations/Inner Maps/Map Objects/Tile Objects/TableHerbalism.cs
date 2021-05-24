using System.Collections.Generic;

public class TableHerbalism : TileObject{
    public TableHerbalism() {
        Initialize(TILE_OBJECT_TYPE.TABLE_HERBALISM);
    }
    public TableHerbalism(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Herbalism Table"; }
}
