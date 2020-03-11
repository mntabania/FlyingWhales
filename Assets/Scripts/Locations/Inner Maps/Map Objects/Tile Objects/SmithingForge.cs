using System.Collections.Generic;

public class SmithingForge : TileObject{
    public SmithingForge() {
        Initialize(TILE_OBJECT_TYPE.SMITHING_FORGE);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public SmithingForge(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
