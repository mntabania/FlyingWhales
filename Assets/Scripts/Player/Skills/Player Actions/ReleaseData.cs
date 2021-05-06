using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class ReleaseData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RELEASE;
    public override string name => "Release";
    public override string description => "This Ability releases a character that has been Restrained, Ensnared, Frozen or Enslaved.";
    public ReleaseData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character targetCharacter) {
            targetCharacter.traitContainer.RemoveRestrainAndImprison(targetCharacter);
            targetCharacter.traitContainer.RemoveStatusAndStacks(targetCharacter, "Frozen");
            targetCharacter.traitContainer.RemoveStatusAndStacks(targetCharacter, "Ensnared");
            targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Enslaved");
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.currentStructure is TortureChambers) {
                return false;
            }
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.currentStructure is TortureChambers) {
            reasons += "Characters inside the Torture Chamber cannot be Released.";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            bool isValid = base.IsValid(target);
            return isValid && targetCharacter.traitContainer.HasTrait("Restrained", "Ensnared", "Frozen", "Enslaved");
        }
        return false;
    }
    #endregion
}
