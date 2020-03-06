using System.Collections.Generic;

public class Sawhorse : TileObject{
    public Sawhorse() {
        Initialize(TILE_OBJECT_TYPE.SAWHORSE);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Sawhorse(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }    
}