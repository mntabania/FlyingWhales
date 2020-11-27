using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class TriggerFlawData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.TRIGGER_FLAW;
    public override string name => "Trigger Flaw";
    public override string description => "This Action can be used to immediately activate an effect of a Villager's negative Trait. You may choose from the Villager's list of flaws that can be triggered.";
    public TriggerFlawData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            UIManager.Instance.characterInfoUI.ShowTriggerFlawUI();
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.limiterComponent.canPerform == false) {
                return false;
            }
            return targetCharacter.isDead == false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            if (!character.isNormalCharacter || character.traitContainer.HasTrait("Cultist") || character.isConsideredRatman) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.limiterComponent.canPerform == false) {
            reasons += "Cannot be used while target is incapacitated,";
        }
        return reasons;
    }
    #endregion
}