using System.Collections.Generic;

public class HerbPlant : TileObject{
    public HerbPlant() {
        Initialize(TILE_OBJECT_TYPE.HERB_PLANT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
    }
    public HerbPlant(SaveDataTileObject data) { }
}
