using Inner_Maps.Location_Structures;

public class SnatchMonsterData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_MONSTER;
    public override string name => "Snatch Monster";
    public override string description => "Snatch a monster and bring them to this structure.";
    public SnatchMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override bool CanPerformAbilityTowards(LocationStructure target) {
        bool canPerform = false;
        if (target is PartyStructure partyStructure) {
            if (partyStructure.IsAvailableForTargeting()) {
                canPerform = true;
            }
        }
        return base.CanPerformAbilityTowards(target) && canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Kennel kennel) {
            bool isValid = base.IsValid(target);
            if (isValid) {
                return kennel.occupyingSummon == null;
            }
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
        reasons += $"Can't snatch because Kennel is occupied\n";
        return reasons;
    }
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowSnatchMonsterUI(structure);
        base.ActivateAbility(structure);
    }
    #endregion
}