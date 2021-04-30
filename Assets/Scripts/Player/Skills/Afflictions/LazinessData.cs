using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class LazinessData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LAZINESS;
    public override string name => "Laziness";
    public override string description => "This Affliction will make a Villager Lazy. Lazy villagers may sometimes refuse to do work and will produce a Chaos Orb whenever they do this.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    public LazinessData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        AfflictPOIWith("Lazy", targetPOI, name);
        OnExecutePlayerSkill();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Lazy")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Lazy")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
}