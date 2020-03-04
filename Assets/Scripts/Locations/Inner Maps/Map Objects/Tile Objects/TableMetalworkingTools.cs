using System.Collections.Generic;

public class TableMetalworkingTools : TileObject{
    public TableMetalworkingTools() {
        Initialize(TILE_OBJECT_TYPE.TABLE_METALWORKING_TOOLS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TableMetalworkingTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
