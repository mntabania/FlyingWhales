using System.Collections.Generic;

public class TableMetalworkingTools : TileObject{
    public TableMetalworkingTools() {
        Initialize(TILE_OBJECT_TYPE.TABLE_METALWORKING_TOOLS);
    }
    public TableMetalworkingTools(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Metalworking Tools"; }
}
