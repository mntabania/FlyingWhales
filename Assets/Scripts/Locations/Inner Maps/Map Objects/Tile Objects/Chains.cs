using System.Collections.Generic;

public class Chains : TileObject {
    public Chains() {
        Initialize(TILE_OBJECT_TYPE.CHAINS);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Chains(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
