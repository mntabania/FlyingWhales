using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class GluttonyData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GLUTTONY;
    public override string name => "Gluttony";
    public override string description => "This Affliction will turn a Villager into a Glutton. Gluttons get hungry faster." +
        "\nA Glutton produces a Chaos Orb each time it eats.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    public GluttonyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        AfflictPOIWith("Glutton", targetPOI, name);
        OnExecutePlayerSkill();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Glutton")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Glutton")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}