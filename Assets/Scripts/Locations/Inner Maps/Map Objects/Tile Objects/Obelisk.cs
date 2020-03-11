using System.Collections.Generic;

public class Obelisk : TileObject{
    public Obelisk() {
        Initialize(TILE_OBJECT_TYPE.OBELISK);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Obelisk(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }    
}