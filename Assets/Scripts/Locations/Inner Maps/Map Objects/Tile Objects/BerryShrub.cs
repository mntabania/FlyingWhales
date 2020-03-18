using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BerryShrub : Crops {

    public BerryShrub() {
        Initialize(TILE_OBJECT_TYPE.BERRY_SHRUB);
    }
    public BerryShrub(SaveDataTileObject data) {
        Initialize(data);
    }

    #region Growth State
    protected override int GetRipeningTicks() {
        return GameManager.Instance.GetTicksBasedOnHour(24);
    }
    #endregion
    
    public override string ToString() {
        return $"Berry Shrub {id.ToString()}";
    }

    
}
