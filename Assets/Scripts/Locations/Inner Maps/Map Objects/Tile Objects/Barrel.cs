using System.Collections.Generic;

public class Barrel : TileObject{
    public Barrel() {
        Initialize(TILE_OBJECT_TYPE.BARREL);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Barrel(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
