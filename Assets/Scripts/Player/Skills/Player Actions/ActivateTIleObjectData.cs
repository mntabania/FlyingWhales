using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class ActivateTileObjectData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.ACTIVATE_TILE_OBJECT;
    public override string name { get { return "Activate Tile Object"; } }
    public override string description { get { return "Activate Tile Object"; } }

    public ActivateTileObjectData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is TileObject targetTileObject) {
            targetTileObject.ActivateTileObject();
        }
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        return true;
    }
    #endregion
}