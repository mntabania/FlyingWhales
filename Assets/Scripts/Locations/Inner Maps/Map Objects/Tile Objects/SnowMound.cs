using System.Collections.Generic;
using Inner_Maps;

public class SnowMound : TileObject{
    public SnowMound() {
        Initialize(TILE_OBJECT_TYPE.SNOW_MOUND, false);
        traitContainer.RemoveTrait(this, "Flammable");
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.EXTRACT_ITEM);
    }
    public SnowMound(SaveDataTileObject data) {
        Initialize(data, false);
        traitContainer.RemoveTrait(this, "Flammable");
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.EXTRACT_ITEM);
    }

    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        traitContainer.RemoveTrait(this, "Melting");
        if (previousTile != null) {
            List<LocationGridTile> tiles = previousTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].genericTileObject.traitContainer.AddTrait(tiles[i].genericTileObject, "Wet");
            }
        }
    }

    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if(gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType != BIOMES.SNOW) {
            traitContainer.AddTrait(this, "Melting");
        } else {
            traitContainer.RemoveTrait(this, "Melting");
        }
    }
}