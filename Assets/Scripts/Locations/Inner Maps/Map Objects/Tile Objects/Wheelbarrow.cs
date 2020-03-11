using System.Collections.Generic;

public class Wheelbarrow : TileObject{
    public Wheelbarrow() {
        Initialize(TILE_OBJECT_TYPE.WHEELBARROW);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public Wheelbarrow(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
