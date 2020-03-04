using System.Collections.Generic;

public class Brazier : TileObject{
    
    public Brazier() {
        Initialize(TILE_OBJECT_TYPE.BRAZIER);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Brazier(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
