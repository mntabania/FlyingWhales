using Inner_Maps.Location_Structures;
public class DrainSpiritData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DRAIN_SPIRIT;
    public override string name => "Drain Spirit";
    public override string description => "This Action can be used on a character to drain it of HP and gain Spirit Energy.";
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
                return targetCharacter.currentStructure is Kennel || targetCharacter.currentStructure is Defiler || targetCharacter.currentStructure is TortureChambers;
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
            return true;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters is already being drained.";
        }
        return reasons;
    }
}
