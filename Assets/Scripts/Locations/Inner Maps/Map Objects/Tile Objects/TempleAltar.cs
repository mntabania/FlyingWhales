using System.Collections.Generic;

public class TempleAltar : TileObject{
    public TempleAltar() {
        Initialize(TILE_OBJECT_TYPE.TEMPLE_ALTAR);
    }
    public TempleAltar(SaveDataTileObject data) : base(data) {
        
    }
}
