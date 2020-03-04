using System.Collections.Generic;

public class RackWeapons : TileObject{
    public RackWeapons() {
        Initialize(TILE_OBJECT_TYPE.RACK_WEAPONS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public RackWeapons(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
