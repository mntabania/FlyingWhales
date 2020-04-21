using System.Collections.Generic;

public class TableMetalworkingTools : TileObject{
    public TableMetalworkingTools() {
        Initialize(TILE_OBJECT_TYPE.TABLE_METALWORKING_TOOLS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TableMetalworkingTools(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Metalworking Tools"; }
}
