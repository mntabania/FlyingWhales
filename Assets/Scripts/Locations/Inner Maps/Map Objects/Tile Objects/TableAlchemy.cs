using System.Collections.Generic;

public class TableAlchemy : TileObject{
    public TableAlchemy() {
        Initialize(TILE_OBJECT_TYPE.TABLE_ALCHEMY);
    }
    public TableAlchemy(SaveDataTileObject data) : base(data){
        
    }
    
    protected override string GenerateName() { return "Alchemy Table"; }
}
