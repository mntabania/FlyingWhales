using System.Collections.Generic;

public class ArcheryTarget : TileObject{
    
    public ArcheryTarget() {
        Initialize(TILE_OBJECT_TYPE.ARCHERY_TARGET);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public ArcheryTarget(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
