using System.Collections.Generic;

public class RackStaves : TileObject{
    public RackStaves() {
        Initialize(TILE_OBJECT_TYPE.RACK_STAVES);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public RackStaves(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
