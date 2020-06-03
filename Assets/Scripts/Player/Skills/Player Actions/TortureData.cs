using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class TortureData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.TORTURE;
    public override string name => "Torture";
    public override string description => $"Torture a {UtilityScripts.Utilities.VillagerIcon()}Villager to afflict it with a random negative Trait and a random negative Status.";
    public TortureData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.ROOM };
        //TODO: Move this when torture has been moved to skill trees
        SetManaCost(10);
    }

    #region Overrides
    public override void ActivateAbility(StructureRoom room) {
        if (room is TortureRoom tortureRoom) {
            tortureRoom.BeginTorture();
        }
        base.ActivateAbility(room);
    }
    public override bool CanPerformAbilityTowards(StructureRoom room) {
        bool canPerform = base.CanPerformAbilityTowards(room);
        if (canPerform) {
            if (room is TortureRoom tortureRoom) {
                return tortureRoom.currentTortureTarget == null && tortureRoom.HasValidTortureTarget();
            }
            return false;
        }
        return canPerform;
    }
    #endregion
}