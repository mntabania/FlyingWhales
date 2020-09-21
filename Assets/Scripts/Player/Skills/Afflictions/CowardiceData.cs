using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class CowardiceData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.COWARDICE;
    public override string name => "Cowardice";
    public override string description => "This Affliction will turn a Villager into a Coward. Cowards will often run away from combat.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    public CowardiceData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Coward");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Coward")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Coward")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}