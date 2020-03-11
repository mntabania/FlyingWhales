using System.Collections.Generic;

public class Trash : TileObject{
    public Trash() {
        Initialize(TILE_OBJECT_TYPE.TRASH);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Trash(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
