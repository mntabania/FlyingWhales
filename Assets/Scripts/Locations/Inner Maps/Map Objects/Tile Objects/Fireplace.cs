using System.Collections.Generic;

public class Fireplace : TileObject{
    public Fireplace() {
        Initialize(TILE_OBJECT_TYPE.FIREPLACE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Fireplace(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
