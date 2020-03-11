using System.Collections.Generic;

public class Cauldron : TileObject{
    public Cauldron() {
        Initialize(TILE_OBJECT_TYPE.CAULDRON);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Cauldron(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
