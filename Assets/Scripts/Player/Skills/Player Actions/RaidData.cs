using Inner_Maps.Location_Structures;

public class RaidData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAID;
    public override string name => "Raid";
    public override string description => "Raid a village.";
    public RaidData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowRaidUI(structure);
        base.ActivateAbility(structure);
    }
    #endregion
}