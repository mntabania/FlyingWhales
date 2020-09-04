using System.Collections.Generic;

public class Brazier : TileObject{
    
    public Brazier() {
        Initialize(TILE_OBJECT_TYPE.BRAZIER);
    }
    public Brazier(SaveDataTileObject data) : base(data) {
        
    }
}
