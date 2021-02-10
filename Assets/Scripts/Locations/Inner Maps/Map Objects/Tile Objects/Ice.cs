using System.Collections.Generic;
using Inner_Maps;

public class Ice : TileObject{
    public Ice() {
        Initialize(TILE_OBJECT_TYPE.ICE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
    }
    public Ice(SaveDataTileObject data) { }
    
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        traitContainer.RemoveTrait(this, "Melting");
        if (previousTile != null) {
            previousTile.genericTileObject.traitContainer.AddTrait(previousTile.genericTileObject, "Wet");
        }
    }

    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if(gridTileLocation.parentArea.biomeType != BIOMES.SNOW) {
            traitContainer.AddTrait(this, "Melting");
        } else {
            traitContainer.RemoveTrait(this, "Melting");
        }
    }
    public override void OnLoadPlacePOI() {
        //override this so that melting trait does not create an additional Melting trait when loading.
        DefaultProcessOnPlacePOI();
    }
}
