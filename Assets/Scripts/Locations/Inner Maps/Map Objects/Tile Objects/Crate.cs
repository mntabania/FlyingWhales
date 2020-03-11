using System.Collections.Generic;

public class Crate : TileObject{
    public Crate() {
        Initialize(TILE_OBJECT_TYPE.CRATE);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Crate(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
