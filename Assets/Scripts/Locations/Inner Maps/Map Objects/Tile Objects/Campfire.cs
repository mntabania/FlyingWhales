using System.Collections.Generic;

public class Campfire : TileObject{
    
    public Campfire() {
        Initialize(TILE_OBJECT_TYPE.CAMPFIRE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Campfire(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
