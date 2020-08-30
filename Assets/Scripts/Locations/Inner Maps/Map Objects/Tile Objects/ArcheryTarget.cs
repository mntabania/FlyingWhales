using System.Collections.Generic;

public class ArcheryTarget : TileObject{
    
    public ArcheryTarget() {
        Initialize(TILE_OBJECT_TYPE.ARCHERY_TARGET);
    }
    public ArcheryTarget(SaveDataTileObject data) {}
}
