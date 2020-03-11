using System.Collections.Generic;

public class Wheelbarrow : TileObject{
    public Wheelbarrow() {
        Initialize(TILE_OBJECT_TYPE.WHEELBARROW);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Wheelbarrow(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
