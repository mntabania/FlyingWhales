using System.Collections.Generic;

public class Torch : TileObject{
    public Torch() {
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        Initialize(TILE_OBJECT_TYPE.TORCH);
        traitContainer.AddTrait(this, "Immovable");
    }
    public Torch(SaveDataTileObject data) : base(data) {
        
    }
}
