using System.Collections.Generic;

public class Anvil : TileObject {
    
    public Anvil() {
        Initialize(TILE_OBJECT_TYPE.ANVIL);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Anvil(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
