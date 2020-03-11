using System.Collections.Generic;

public class Shelf : TileObject{
    public Shelf() {
        Initialize(TILE_OBJECT_TYPE.SHELF);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Shelf(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
