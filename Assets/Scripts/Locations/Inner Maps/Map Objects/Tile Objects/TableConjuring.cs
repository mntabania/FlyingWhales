using System.Collections.Generic;

public class TableConjuring : TileObject{
    public TableConjuring() {
        Initialize(TILE_OBJECT_TYPE.TABLE_CONJURING);
    }
    public TableConjuring(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Conjuring Table"; }
}
