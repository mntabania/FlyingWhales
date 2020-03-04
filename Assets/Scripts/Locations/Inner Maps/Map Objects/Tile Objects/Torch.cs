using System.Collections.Generic;

public class Torch : TileObject{
    public Torch() {
        Initialize(TILE_OBJECT_TYPE.TORCH);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Torch(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
