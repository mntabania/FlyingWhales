using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class PyrophobiaData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PYROPHOBIA;
    public override string name => "Pyrophobia";
    public override string description => "This Affliction will make a Villager fear fire. Pyrophobic Villagers will always flee or cower whenever it gets close to a fire.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    public PyrophobiaData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        AfflictPOIWith("Pyrophobic", targetPOI, name);
        OnExecuteSpellActionAffliction();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Pyrophobic")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Pyrophobic")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}