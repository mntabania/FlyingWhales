using System;
using System.Collections.Generic;
using Locations.Area_Features;

public class CornCrop : Crops {
    
    public override System.Type serializedData => typeof(SaveDataCornCrop);
    public override bool doesNotGrowPerTick => true;
    public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.CORN;

    public CornCrop() : base() {
        Initialize(TILE_OBJECT_TYPE.CORN_CROP);
        //AddAdvertisedAction(INTERACTION_TYPE.TEND);
    }
    public CornCrop(SaveDataCornCrop data) : base(data) { }

    #region Growth State
    public override int GetRipeningTicks() {
        int ticks;
        if (gridTileLocation.area.featureComponent.HasFeature(AreaFeatureDB.Fertile_Feature)) {
            ticks = GameManager.Instance.GetTicksBasedOnHour(96);
        } else {
            ticks = GameManager.Instance.GetTicksBasedOnHour(120);
        }
        return ticks;
    }
    #endregion

}

#region Save Data
public class SaveDataCornCrop : SaveDataCrops { }
#endregion