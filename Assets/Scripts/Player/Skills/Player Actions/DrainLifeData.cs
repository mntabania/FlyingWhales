using Inner_Maps.Location_Structures;
public class DrainLifeData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DRAIN_LIFE;
    public override string name => "Drain Life";
    public override string description => "This Action can be used on a character to drain it of HP and gain Chaotic Energy.";
    public DrainLifeData() : base() {
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
                if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
                    return false;
                }
                return targetCharacter.currentStructure is Kennel || targetCharacter.currentStructure is Defiler || targetCharacter.currentStructure is TortureChambers;
            }
        }
        return false;
    }
}
