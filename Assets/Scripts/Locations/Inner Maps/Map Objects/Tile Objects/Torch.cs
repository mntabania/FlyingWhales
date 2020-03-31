using System.Collections.Generic;

public class Torch : TileObject{
    public Torch() {
        Initialize(TILE_OBJECT_TYPE.TORCH);
        traitContainer.AddTrait(this, "Immovable");
    }
    public Torch(SaveDataTileObject data) {
        Initialize(data);
        traitContainer.AddTrait(this, "Immovable");
    }
}
