using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mushroom : Crops {

    public Mushroom() {
        Initialize(TILE_OBJECT_TYPE.BERRY_SHRUB);
    }
    public Mushroom(SaveDataTileObject data) {
        Initialize(data);
    }

    #region Growth State
    protected override int GetRipeningTicks() {
        return GameManager.Instance.GetTicksBasedOnHour(48);
    }
    #endregion
    
    public override string ToString() {
        return $"Mushroom {id.ToString()}";
    }
}
