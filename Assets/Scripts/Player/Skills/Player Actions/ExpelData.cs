using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using UtilityScripts;
using Object_Pools;

public class ExpelData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EXPEL;
    public override string name => "Expel";
    public override string description => "This Ability kicks out a character from its current Village and Faction." +
        "\nExpelling a hostile Villager from its Faction will produce a Chaos Orb.";
    public ExpelData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            targetCharacter.jobQueue.CancelAllJobs();
            if(targetCharacter.faction != null) {
                targetCharacter.faction.KickOutCharacter(targetCharacter);
            }
            if (targetCharacter.homeSettlement != null && targetCharacter.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
                targetCharacter.MigrateHomeStructureTo(null);
            }
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Skills", "Expel", "expel_success", null, LogUtilities.Player_Life_Changes_Tags);
            log.AddToFillers(null, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, targetCharacter.prevFaction.name, LOG_IDENTIFIER.FACTION_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            LogPool.Release(log);
            Messenger.Broadcast(PlayerSkillSignals.EXPEL_ACTIVATED, targetPOI);
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            bool isValid = base.IsValid(target);
            return isValid && targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer/* && targetCharacter.faction.IsFriendlyWith(PlayerManager.Instance.player.playerFaction)*/;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}
