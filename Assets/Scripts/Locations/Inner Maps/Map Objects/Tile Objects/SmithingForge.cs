using System.Collections.Generic;

public class SmithingForge : TileObject{
    public SmithingForge() {
        Initialize(TILE_OBJECT_TYPE.SMITHING_FORGE);
    }
    public SmithingForge(SaveDataTileObject data) : base(data) {
        
    }
}
