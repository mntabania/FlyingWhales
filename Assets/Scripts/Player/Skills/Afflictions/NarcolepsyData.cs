using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class NarcolepsyData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.NARCOLEPSY;
    public override string name { get { return "Narcolepsy"; } }
    public override string description { get { return "The target will gain a sleep disorder that causes it to have sudden attacks of sleep."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }

    public NarcolepsyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Narcoleptic");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Narcoleptic")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}