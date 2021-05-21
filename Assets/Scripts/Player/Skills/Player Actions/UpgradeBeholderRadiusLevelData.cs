using Inner_Maps.Location_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeBeholderRadiusLevelData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL;
    public override string name => "Increase Radius";
    public override string description => GetDescription();

    private Snooper m_targetBeholder;
    public string GetDescription() {
        if (m_targetBeholder != null) {
            if (m_targetBeholder.GetRadiusLevel() >= 3) {
                return "Increase the radius of each Demon Eye of this Snooper";
            }
            return $"Spend {EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_targetBeholder.GetRadiusLevel()).GetCostStringWithIcon()} to increase radius by 1.";
        } else {
            return "Increase the radius of each Demon Eye of this Snooper";
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure target) {
        bool canPerform = false;
        m_targetBeholder = target as Snooper;
        if (m_targetBeholder.GetRadiusLevel() < 3) {
            canPerform = true;
        } else {
            canPerform = false;
        }

        if (m_targetBeholder.GetRadiusLevel() < 3 && PlayerManager.Instance.player.chaoticEnergy >= EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_targetBeholder.GetRadiusLevel()).processedAmount && canPerform) {
            canPerform = true;
        } else {
            canPerform = false;
        }

        return base.CanPerformAbilityTowards(target) && canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Snooper) {
            return true;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
        m_targetBeholder = structure as Snooper;
        if (m_targetBeholder.GetRadiusLevel() >= 3) {
            reasons += $"Snooper Radius already max level\n";
            return reasons;
        }
        if (PlayerManager.Instance.player.chaoticEnergy < EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_targetBeholder.GetRadiusLevel()).processedAmount) {
            reasons += $"Not enough chaotic energy.\n";
            return reasons;
        }

        return reasons;
    }

    public UpgradeBeholderRadiusLevelData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        (structure as Snooper).LevelUpRadius();
        base.ActivateAbility(structure);
    }
    #endregion
}