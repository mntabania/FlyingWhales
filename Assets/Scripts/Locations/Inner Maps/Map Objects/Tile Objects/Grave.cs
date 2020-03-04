using System.Collections.Generic;

public class Grave : TileObject{
    public Grave() {
        Initialize(TILE_OBJECT_TYPE.GRAVE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Grave(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
