using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class MusicHaterData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MUSIC_HATER;
    public override string name => "Music Hater";
    public override string description => "This Affliction will make a Villager hate anything related to Music.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

    public MusicHaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        AfflictPOIWith("Music Hater", targetPOI, name);
        OnExecutePlayerSkill();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Music Lover") || targetCharacter.traitContainer.HasTrait("Music Hater")) {
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