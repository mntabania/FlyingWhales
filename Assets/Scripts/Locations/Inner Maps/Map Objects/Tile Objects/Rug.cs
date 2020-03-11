using System.Collections.Generic;

public class Rug : TileObject{
     public Rug() { 
         Initialize(TILE_OBJECT_TYPE.RUG);
         advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
     }
     public Rug(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
     }
        
}
