using System;
using System.Collections.Generic;

public class CornCrop : Crops{
    public CornCrop() {
        Initialize(TILE_OBJECT_TYPE.CORN_CROP);
    }
    public CornCrop(SaveDataTileObject data) {
        Initialize(data);
    }

    #region Growth State
    protected override int GetRipeningTicks() {
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap 
            && gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.
                featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
            return GameManager.Instance.GetTicksBasedOnHour(48);
        } else {
            return GameManager.Instance.GetTicksBasedOnHour(96);
        }
    }
    #endregion

}