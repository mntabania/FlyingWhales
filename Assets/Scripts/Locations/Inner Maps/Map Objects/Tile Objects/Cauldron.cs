using System.Collections.Generic;

public class Cauldron : TileObject{
    public Cauldron() {
        Initialize(TILE_OBJECT_TYPE.CAULDRON);
    }
    public Cauldron(SaveDataTileObject data) : base(data) {
        
    }
}
