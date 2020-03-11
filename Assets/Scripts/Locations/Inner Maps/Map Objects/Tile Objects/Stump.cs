using System.Collections.Generic;

public class Stump : TileObject{
    public Stump() {
        Initialize(TILE_OBJECT_TYPE.STUMP);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Stump(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
