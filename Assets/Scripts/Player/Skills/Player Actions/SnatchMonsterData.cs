using Inner_Maps.Location_Structures;

public class SnatchMonsterData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_MONSTER;
    public override string name => "Snatch Monster";
    public override string description => "Snatch a monster and bring them to this structure.";
    public SnatchMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowSnatchMonsterUI(structure);
        base.ActivateAbility(structure);
    }
    #endregion
}