﻿using System;
using System.Collections.Generic;
using Locations.Features;

public class CornCrop : Crops {
    public CornCrop() : base() {
        Initialize(TILE_OBJECT_TYPE.CORN_CROP);
        AddAdvertisedAction(INTERACTION_TYPE.TEND);
    }
    public CornCrop(SaveDataTileObject data) {
        Initialize(data);
        AddAdvertisedAction(INTERACTION_TYPE.TEND);
    }

    #region Growth State
    public override int GetRipeningTicks() {
        int ticks;
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap 
            && gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.
                featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
            ticks = GameManager.Instance.GetTicksBasedOnHour(96);
        } else {
            ticks = GameManager.Instance.GetTicksBasedOnHour(120);
        }
        return ticks;
    }
    #endregion

}