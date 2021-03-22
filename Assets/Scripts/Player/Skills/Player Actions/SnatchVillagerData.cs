using Inner_Maps.Location_Structures;

public class SnatchVillagerData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_VILLAGER;
    public override string name => "Snatch Villager";
    public override string description => "Snatch a villager and bring them to this structure.";
    public SnatchVillagerData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowSnatchVillagerUI(structure);
        base.ActivateAbility(structure);
    }
    #endregion
}