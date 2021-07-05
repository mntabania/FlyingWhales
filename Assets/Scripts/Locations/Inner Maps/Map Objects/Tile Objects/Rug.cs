using System.Collections.Generic;

public class Rug : TileObject{
    public Rug() {
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        Initialize(TILE_OBJECT_TYPE.RUG);
    }
    public Rug(SaveDataTileObject data) : base(data) {
        
    }

}
