using System.Collections.Generic;

public class Flower : TileObject{
    public Flower() {
        Initialize(TILE_OBJECT_TYPE.FLOWER);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Flower(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
