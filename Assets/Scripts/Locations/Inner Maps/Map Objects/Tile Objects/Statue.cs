using System.Collections.Generic;

public class Statue : TileObject{
    public Statue() {
        Initialize(TILE_OBJECT_TYPE.STATUE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Statue(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
