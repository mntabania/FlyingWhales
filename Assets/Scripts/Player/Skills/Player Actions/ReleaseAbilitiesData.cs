using Inner_Maps.Location_Structures;

public class ReleaseAbilitiesData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RELEASE_ABILITIES;
    public override string name => "Release Abilities";
    public override string description => "Release your abilities.";
    public ReleaseAbilitiesData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is ThePortal) {
            UIManager.Instance.ShowPurchaseSkillUI();    
        }
        base.ActivateAbility(structure);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom || PlayerSkillManager.Instance.unlockAllSkills;
        }
        return false;
    }
    #endregion
}