using System.Collections.Generic;

public class Wheelbarrow : TileObject{
    public Wheelbarrow() {
        Initialize(TILE_OBJECT_TYPE.WHEELBARROW);
    }
    public Wheelbarrow(SaveDataTileObject data) : base(data) {
        
    }
}
