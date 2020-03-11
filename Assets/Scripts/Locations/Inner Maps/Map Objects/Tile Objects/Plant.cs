using System.Collections.Generic;

public class Plant : TileObject{
    public Plant() {
        Initialize(TILE_OBJECT_TYPE.PLANT);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Plant(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
