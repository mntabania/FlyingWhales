using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class MusicHaterData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.MUSIC_HATER;
    public override string name { get { return "Music Hater"; } }
    public override string description { get { return "Music Hater"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }

    public MusicHaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Music Hater");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Music Hater", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Music Lover")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Music Hater")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        } else if (targetCharacter.traitContainer.HasTrait("Music Lover")) {
            reasons += $"{targetCharacter.name} is already a Music Lover,";
        }
        return reasons;
    }
    #endregion
}