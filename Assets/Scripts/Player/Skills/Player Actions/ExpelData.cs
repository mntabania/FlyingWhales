using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class ExpelData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EXPEL;
    public override string name => "Expel";
    public override string description => "This Action kicks out a character from its current Village and Faction. Available because its Faction is allied to you.";
    public ExpelData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            if(targetCharacter.faction != null) {
                targetCharacter.faction.KickOutCharacter(targetCharacter);
            }
            if (targetCharacter.homeSettlement != null && targetCharacter.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
                targetCharacter.MigrateHomeStructureTo(null);
            }
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            return targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer && targetCharacter.faction.IsFriendlyWith(PlayerManager.Instance.player.playerFaction);
        }
        return false;
    }
    #endregion
}
