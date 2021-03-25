using Inner_Maps.Location_Structures;
public class DrainSpiritData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DRAIN_SPIRIT;
    public override string name => "Drain Spirit";
    public override string description => "This Ability slowly kills the target to drain them of their Spirit Energy.";
    public DrainSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            targetCharacter.traitContainer.AddTrait(targetCharacter, "Being Drained");
            base.ActivateAbility(targetPOI);    
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                if (targetCharacter.isDead) {
                    return false;
                }
                if (targetCharacter is Summon) {
                    return targetCharacter.currentStructure is Kennel;
                } else {
                    return targetCharacter.currentStructure is TortureChambers;
                }
            }
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerformAbility = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerformAbility) {
            if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
                return false;
            }
            if (targetCharacter.interruptComponent.isInterrupted) {
                if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed ||
                    targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                    //do not allow characters being tortured or brainwashed to be seized
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Character is already being drained.";
        }
        if (targetCharacter.interruptComponent.isInterrupted) {
            if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed) {
                reasons += "Character is currently being Brainwashed.";
            }else if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                reasons += "Character is currently being Tortured.";
            }
        }
        return reasons;
    }
}
