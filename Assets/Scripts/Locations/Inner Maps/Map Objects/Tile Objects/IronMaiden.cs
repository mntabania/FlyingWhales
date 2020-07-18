using System.Collections.Generic;

public class IronMaiden : TileObject {
    
    public IronMaiden() {
        Initialize(TILE_OBJECT_TYPE.IRON_MAIDEN);
    }
    public IronMaiden(SaveDataTileObject data) {
        Initialize(data);
    }
}
