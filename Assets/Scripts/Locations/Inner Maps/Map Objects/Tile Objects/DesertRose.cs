using System.Collections.Generic;

public class DesertRose : TileObject{
    public DesertRose() {
        Initialize(TILE_OBJECT_TYPE.DESERT_ROSE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public DesertRose(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

    public void DesertRoseEffect() {
        if(gridTileLocation != null) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.ChangeBiomeType(BIOMES.DESERT);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
}
