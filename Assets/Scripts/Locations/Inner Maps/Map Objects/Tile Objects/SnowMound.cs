using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;
public class SnowMound : TileObject{
    public SnowMound() {
        Initialize(TILE_OBJECT_TYPE.SNOW_MOUND, false);
        traitContainer.RemoveTrait(this, "Flammable");
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.EXTRACT_ITEM);
    }
    public SnowMound(SaveDataTileObject data) : base(data) { }

    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        traitContainer.RemoveTrait(this, "Melting");
        if (previousTile != null) {
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            previousTile.PopulateTilesInRadius(tiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].tileObjectComponent.genericTileObject.traitContainer.AddTrait(tiles[i].tileObjectComponent.genericTileObject, "Wet");
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
        }
    }

    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if(gridTileLocation.mainBiomeType != BIOMES.SNOW) {
            traitContainer.AddTrait(this, "Melting");
        } else {
            traitContainer.RemoveTrait(this, "Melting");
        }
    }
}