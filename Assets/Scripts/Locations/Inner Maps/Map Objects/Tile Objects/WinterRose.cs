using System.Collections.Generic;

public class WinterRose : TileObject{
    public WinterRose() {
        Initialize(TILE_OBJECT_TYPE.WINTER_ROSE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public WinterRose(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

    public void WinterRoseEffect() {
        if(gridTileLocation != null) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.ChangeBiomeType(BIOMES.SNOW);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
}
