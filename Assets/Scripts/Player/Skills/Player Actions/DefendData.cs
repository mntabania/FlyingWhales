using Inner_Maps.Location_Structures;

public class DefendData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEFEND;
    public override string name => "Defend";
    public override string description => "Summon minions to defend around the Prism.";
    public DefendData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowDefendUI(structure);
        base.ActivateAbility(structure);
    }
    #endregion
}