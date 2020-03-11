using System.Collections.Generic;

public class RackFarmingTools : TileObject{
    public RackFarmingTools() {
        Initialize(TILE_OBJECT_TYPE.RACK_FARMING_TOOLS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public RackFarmingTools(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
