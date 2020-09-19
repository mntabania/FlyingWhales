using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class NarcolepsyData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.NARCOLEPSY;
    public override string name => "Narcolepsy";
    public override string description => "This Affliction will make a Villager Narcoleptic. Narcoleptic villagers may involuntarily fall asleep at any time.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    public NarcolepsyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Narcoleptic");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Narcoleptic")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Narcoleptic")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}