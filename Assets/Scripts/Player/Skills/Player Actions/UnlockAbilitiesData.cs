using Inner_Maps.Location_Structures;

public class UnlockAbilitiesData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UNLOCK_ABILITIES;
    public override string name => "Unlock Abilities";
    public override string description => "Unlock your abilities.";
    public UnlockAbilitiesData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        UIManager.Instance.ShowUnlockAbilitiesUI();
        base.ActivateAbility(structure);
    }
    #endregion
}