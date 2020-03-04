using System.Collections.Generic;

public class IronMaiden : TileObject {
    
    public IronMaiden() {
        Initialize(TILE_OBJECT_TYPE.IRON_MAIDEN);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public IronMaiden(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
    public override bool CanBeDamaged() {
        //prevent iron maiden in torture chamber from being damaged.
        return structureLocation != null 
               && structureLocation.structureType != STRUCTURE_TYPE.TORTURE_CHAMBER; 
    }
}
