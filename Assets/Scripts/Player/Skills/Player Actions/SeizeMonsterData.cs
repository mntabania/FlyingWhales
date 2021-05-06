using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SeizeMonsterData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SEIZE_MONSTER;
    public override string name => "Seize Monster";
    public override string description => $"This Ability can be used to take a Monster and then transfer it to an unoccupied tile.";
    public SeizeMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.race == RACE.TRITON) {
                return false;
            }
            return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !targetCharacter.traitContainer.HasTrait("Hibernating") && !targetCharacter.traitContainer.HasTrait("Being Drained");
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.race == RACE.TRITON) {
            reasons += "Tritons cannot be seized,";
        } else if (targetCharacter.traitContainer.HasTrait("Hibernating")) {
            if (targetCharacter is Golem) {
                reasons += "Hibernating golems cannot be seized.";
            } else {
                reasons += "Hibernating characters cannot be seized.";    
            }
        } else if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters being drained cannot be seized.";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            bool isValid = base.IsValid(target);
            return isValid && !(targetCharacter is Dragon);
        }
        return false;
    }
    #endregion
}