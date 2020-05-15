using System.Collections.Generic;

public class HerbPlant : TileObject{
    public HerbPlant() {
        Initialize(TILE_OBJECT_TYPE.HERB_PLANT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
    }
    public HerbPlant(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
    }
}
