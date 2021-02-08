using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class PoisonData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON;
    public override string name => "Poison";
    public override string description => "This Action can be used to apply Poisoned on an object.";
    public PoisonData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        targetPOI.traitContainer.AddTrait(targetPOI, "Poisoned");
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", name, "activated", null, LOG_TAG.Player);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.traitContainer.HasTrait("Poisoned", "Robust")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    #endregion
}