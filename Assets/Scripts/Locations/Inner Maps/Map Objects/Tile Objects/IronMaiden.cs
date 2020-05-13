using System.Collections.Generic;

public class IronMaiden : TileObject {
    
    public IronMaiden() {
        Initialize(TILE_OBJECT_TYPE.IRON_MAIDEN);
    }
    public IronMaiden(SaveDataTileObject data) {
        Initialize(data);
    }
    public override bool CanBeDamaged() {
        //prevent iron maiden in torture chamber from being damaged.
        return structureLocation != null 
               && structureLocation.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS; 
    }
}
