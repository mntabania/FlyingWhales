using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class RepairData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.REPAIR;
    public override string name => "Repair";
    public override string description => "This Action can be used to fully repair all damages to a demonic structure.";
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
    #endregion
}
