using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mushroom : Crops {

    public Mushroom() {
        Initialize(TILE_OBJECT_TYPE.MUSHROOM);
    }
    public Mushroom(SaveDataTileObject data) {
        Initialize(data);
    }

    #region Override
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddPlayerAction(SPELL_TYPE.PLANT_GERM);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        SetGrowthState(Growth_State.Ripe);
    }
    #endregion

    #region Growth State
    protected override int GetRipeningTicks() {
        return GameManager.Instance.GetTicksBasedOnHour(48);
    }
    #endregion
    
    public override string ToString() {
        return $"Mushroom {id.ToString()}";
    }
}
