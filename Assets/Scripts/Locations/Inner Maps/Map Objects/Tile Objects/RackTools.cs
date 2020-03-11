using System.Collections.Generic;

public class RackTools : TileObject{
    public RackTools() {
        Initialize(TILE_OBJECT_TYPE.RACK_TOOLS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public RackTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
