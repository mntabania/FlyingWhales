using System.Collections.Generic;

public class SmithingForge : TileObject{
    public SmithingForge() {
        Initialize(TILE_OBJECT_TYPE.SMITHING_FORGE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public SmithingForge(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
