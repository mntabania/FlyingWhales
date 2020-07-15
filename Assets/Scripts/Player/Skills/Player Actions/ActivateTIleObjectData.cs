using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class ActivateData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.ACTIVATE;
    public override string name => "Activate";
    public override string description => "This Action can be used on a few special objects. The effect varies depending on the object but it usually only affects nearby tiles and characters. You've got to try it out first to find out.";
    public ActivateData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is TileObject targetTileObject) {
            targetTileObject.ActivateTileObject();
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject targetTileObject) {
        bool canPerform = base.CanPerformAbilityTowards(targetTileObject);
        if (canPerform) {
            return targetTileObject.gridTileLocation != null;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is AnkhOfAnubis) {
            return false;
        }
        return base.IsValid(target);
    }
    #endregion
}