using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class RepairData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REPAIR;
    public override string name => "Repair";
    public override string description => "This Ability can be used to repair Demonic Structure damage.";
    public RepairData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if(structure.currentHP < structure.maxHP) {
            if (structure is DemonicStructure demonicStructure) {
                demonicStructure.RepairStructure();
            }
            //else if (structure is ManMadeStructure manMadeStructure) {
            //    manMadeStructure.RepairStructure();
            //}
            base.ActivateAbility(structure);
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            return (structure is DemonicStructure /*|| structure is ManMadeStructure*/) && structure.currentHP < structure.maxHP;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is ThePortal) {
            //Portal has high hp but cannot be repaired
            return false;
        }
        if (target is LocationStructure structure) {
            if (structure.hasBeenDestroyed || structure.tiles.Count <= 0 || structure.currentHP >= structure.maxHP) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    #endregion
}
