using System.Collections.Generic;

public class TempleAltar : TileObject{
    public TempleAltar() {
        Initialize(TILE_OBJECT_TYPE.TEMPLE_ALTAR);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TempleAltar(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
