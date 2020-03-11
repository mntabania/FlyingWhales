using System.Collections.Generic;

public class Bandages : TileObject {
    public Bandages() {
        Initialize(TILE_OBJECT_TYPE.BANDAGES);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Bandages(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
