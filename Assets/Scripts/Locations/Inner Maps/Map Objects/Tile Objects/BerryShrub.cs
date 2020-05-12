using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BerryShrub : Crops {

    public BerryShrub() : base() {
        Initialize(TILE_OBJECT_TYPE.BERRY_SHRUB);
    }
    public BerryShrub(SaveDataTileObject data) {
        Initialize(data);
    }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddPlayerAction(SPELL_TYPE.PLANT_GERM);
    }
    #endregion

    #region Growth State
    public override int GetRipeningTicks() {
        return GameManager.Instance.GetTicksBasedOnHour(24);
    }
    #endregion
    
    public override string ToString() {
        return $"Berry Shrub {id.ToString()}";
    }

    
}
