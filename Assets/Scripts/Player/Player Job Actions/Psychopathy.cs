using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Psychopathy : PlayerSpell {

    public Psychopathy() : base(SPELL_TYPE.PSYCHOPATHY) {
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
        //abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character currTarget = targetPOI as Character;
            if (CanPerformActionTowards(currTarget)) {
                Trait newTrait = new Psychopath();
                newTrait.SetLevel(level);
                currTarget.traitContainer.AddTrait(currTarget, newTrait);
            }
            base.ActivateAction(targetPOI);
        }
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Beast") /*targetCharacter.role.roleType == CHARACTER_ROLE.BEAST*/) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Psychopath")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Beast") /*targetCharacter.role.roleType == CHARACTER_ROLE.BEAST*/) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Psychopath")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return true;
    }
}

public class PsychopathyData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.PSYCHOPATHY;
    public override string name { get { return "Psychopathy"; } }
    public override string description { get { return "Turns a character into a serial killer."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MONSTER; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

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
    #endregion
}