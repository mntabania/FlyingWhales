using System.Collections.Generic;

public class Chains : TileObject {
    public Chains() {
        Initialize(TILE_OBJECT_TYPE.CHAINS);
    }
    public Chains(SaveDataTileObject data) : base(data) {
        
    }
}
