using Inner_Maps.Location_Structures;
using System;
using UnityEngine;

public class SnatchVillagerData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_VILLAGER;
    public override string name => "Snatch Villager";
    public override string description => "Snatch a villager and bring them to this structure.";

    public override bool CanPerformAbilityTowards(LocationStructure target) {
        bool canPerform = false;
        if (target is PartyStructure partyStructure) {
            if (partyStructure.IsAvailableForTargeting()) {
                canPerform = true;
            }
        }
        return base.CanPerformAbilityTowards(target) && canPerform;
    }

    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
        reasons += $"Can't build snatch party, Prison is occupied\n";
        return reasons;
    }

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