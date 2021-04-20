using Inner_Maps.Location_Structures;

public class UpgradePortalData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_PORTAL;
    public override string name => "Upgrade";
    public override string description => $"Upgrade the Portal to permanently unlock new Powers.";
    public UpgradePortalData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is ThePortal portal && !portal.IsMaxLevel()) {
            UIManager.Instance.ShowUpgradePortalUI(portal);
        }
        base.ActivateAbility(structure);
    }
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        bool canPerform = base.CanPerformAbilityTowards(targetStructure);
        if (canPerform) {
            if (targetStructure is ThePortal portal) {
                return !portal.IsMaxLevel();
            } 
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
        if (p_targetStructure is ThePortal portal) {
            if (portal.IsMaxLevel()) {
                reasons += $"Portal is at Max Level,";
            }
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom;
        }
        return false;
    }
    #endregion
}