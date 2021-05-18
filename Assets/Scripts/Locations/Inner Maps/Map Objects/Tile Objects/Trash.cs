using System.Collections.Generic;

public class Trash : TileObject{
    public Trash() {
        Initialize(TILE_OBJECT_TYPE.TRASH);
        RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
    }
    public Trash(SaveDataTileObject data) : base(data) {
        
    }
}
