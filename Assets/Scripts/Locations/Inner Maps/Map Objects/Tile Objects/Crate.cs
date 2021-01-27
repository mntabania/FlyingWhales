using System.Collections.Generic;

public class Crate : TileObject{
    public Crate() {
        Initialize(TILE_OBJECT_TYPE.CRATE);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
    }
    public Crate(SaveDataTileObject data) : base(data) {
        
    }
}
