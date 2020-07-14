using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class Mushroom : Crops {

    public Mushroom() : base() {
        Initialize(TILE_OBJECT_TYPE.MUSHROOM);
    }
    public Mushroom(SaveDataTileObject data) {
        Initialize(data);
    }

    #region Override
    public override void SetGrowthState(Growth_State growthState) {
        base.SetGrowthState(growthState);
        if (growthState == Growth_State.Ripe && GameUtilities.RollChance(3)) {
            traitContainer.AddTrait(this, "Abomination Germ");
        }
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (GameManager.Instance.gameHasStarted == false) { //set mushroom as ripe on its initial placement
            SetGrowthState(Growth_State.Ripe);    
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
