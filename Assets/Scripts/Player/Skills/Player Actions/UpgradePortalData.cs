using Inner_Maps.Location_Structures;

public class UpgradePortalData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE;
    public override string name => "Upgrade Portal";
    public override string description => $"Upgrade your portal.";
    public UpgradePortalData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is Biolab biolab) {
            UIManager.Instance.ShowBiolabUI();
        }
        base.ActivateAbility(structure);
    }
    #endregion
}