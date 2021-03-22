using Inner_Maps.Location_Structures;

public class UpgradeAbilitiesData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_ABILITIES;
    public override string name => "Upgrade Abilities";
    public override string description => "Upgrade your abilities.";
    public UpgradeAbilitiesData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowUpgradeAbilitiesUI();
        base.ActivateAbility(structure);
    }
    #endregion
}