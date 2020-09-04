using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsychopathyData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.PSYCHOPATHY;
    public override string name => "Psychopathy";
    public override string description => "This Affliction will turn a Villager into a Serial Killer. Serial Killers have a specific type of victim that they would target for abduction and killing. You may set your desired Victim Profile.";
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public PsychopathyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        UIManager.Instance.psychopathUI.ShowPsychopathUI(targetPOI as Character);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Psychopath") || targetCharacter.traitContainer.HasTrait("Beast")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Psychopath")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}