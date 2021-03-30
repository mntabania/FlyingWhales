using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class DestroyStructureData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY_STRUCTURE;
    public override string name => "Destroy";
    public override string description => "This Ability can be used to destroy Demonic Structure.";
    public DestroyStructureData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is DemonicStructure demonicStructure) {
            demonicStructure.AdjustHP(-structure.currentHP);
        }
        base.ActivateAbility(structure);
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            return !structure.hasBeenDestroyed && structure.tiles.Count > 0 && structure.currentHP > 0;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is DemonicStructure structure) {
            if (structure.structureType == STRUCTURE_TYPE.THE_PORTAL) {
                //Cannot destroy portal
                return false;
            }
            if (structure.hasBeenDestroyed || structure.tiles.Count <= 0) {
                return false;
            }
        } else {
            return false;
        }
        return base.IsValid(target);
    }
    #endregion
}
