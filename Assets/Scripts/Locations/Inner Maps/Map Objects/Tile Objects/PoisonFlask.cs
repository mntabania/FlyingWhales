using System.Collections.Generic;

public class PoisonFlask : TileObject{
    public PoisonFlask() {
        Initialize(TILE_OBJECT_TYPE.POISON_FLASK, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
    }
    public PoisonFlask(SaveDataTileObject data) : base(data) { }
}
