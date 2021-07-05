using System.Collections.Generic;

public class Shelf : TileObject{
    public Shelf() {
        Initialize(TILE_OBJECT_TYPE.SHELF);
    }
    public Shelf(SaveDataTileObject data) : base(data) {
        
    }
}
