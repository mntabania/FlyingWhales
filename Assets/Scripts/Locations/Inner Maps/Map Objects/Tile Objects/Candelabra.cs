using System.Collections.Generic;

public class Candelabra : TileObject{
    
    public Candelabra() {
        Initialize(TILE_OBJECT_TYPE.CANDELABRA);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Candelabra(SaveDataTileObject data) {
        Initialize(data);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
}
