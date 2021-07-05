using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class Mushroom : Crops {

    public override System.Type serializedData => typeof(SaveDataMushroom);
    public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.VEGETABLES;
    
    public Mushroom() : base() {
        Initialize(TILE_OBJECT_TYPE.MUSHROOM);
    }
    public Mushroom(SaveDataMushroom data) : base(data) { }

    #region Initialization
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
        SetGrowthState(Growth_State.Ripe);
    }
    #endregion
    
    #region Override
    public override void SetGrowthState(Growth_State growthState) {
        base.SetGrowthState(growthState);
        if (growthState == Growth_State.Ripe && GameUtilities.RollChance(3)) {
            traitContainer.AddTrait(this, "Abomination Germ");
        }
    }
    #endregion

    #region Growth State
    public override int GetRipeningTicks() {
        return GameManager.Instance.GetTicksBasedOnHour(48);
    }
    #endregion
    
    public override string ToString() {
        return $"Mushroom {id.ToString()}";
    }
}
#region Save Data
public class SaveDataMushroom : SaveDataCrops { }
#endregion