using Inner_Maps.Location_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnatchVillagerData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_VILLAGER;
    public override string name => "Snatch Villager";
    public override string description => "Capture and imprison a Villager. It is easier to Snatch a Villager while it is alone and sleeping.";

    public override bool CanPerformAbilityTowards(LocationStructure target) {
        bool canPerform = false;
        if (target is PartyStructure partyStructure) {
            if (partyStructure.IsAvailableForTargeting()) {
                canPerform = true;
            }
        }
        return base.CanPerformAbilityTowards(target) && canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is TortureChambers tortureChambers) {
            if (tortureChambers.rooms != null && tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
                List<Character> charactersInRoom = prisonCell.charactersInRoom;
                if (!charactersInRoom.Any(c => prisonCell.IsValidOccupant(c))) {
                    //if prison cell does not have any valid occupants yet, then allow snatch action.
                    return true;    
                }
            }
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
        reasons += $"Can't snatch because Prison is occupied\n";
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