using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class BrainwashData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.BRAINWASH;
    public override string name => "Brainwash";
    public override string description => $"Brainwash a {UtilityScripts.Utilities.VillagerIcon()}Villager.";
    
    public BrainwashData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.ROOM };
    }

    #region Overrides
    public override void ActivateAbility(StructureRoom room) {
        if (room is DefilerRoom defilerRoom) {
            defilerRoom.StartBrainwash();
        }
        base.ActivateAbility(room);
    }
    public override bool CanPerformAbilityTowards(StructureRoom room) {
        bool canPerform = base.CanPerformAbilityTowards(room);
        if (canPerform) {
            if (room is DefilerRoom defilerRoom) {
                return defilerRoom.currentBrainwashTarget == null && defilerRoom.HasValidBrainwashTarget();
            }
            return false;
        }
        return false;
    }
    #endregion
}