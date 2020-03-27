using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class TortureData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.TORTURE;
    public override string name { get { return "Torture"; } }
    public override string description { get { return "Torture"; } }

    public TortureData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is TortureChamber tortureChamber) {
            tortureChamber.ChooseTortureTarget();
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            if (structure is TortureChamber tortureChamber) {
                return tortureChamber.currentTortureTarget == null;
            }
            return false;
        }
        return canPerform;
    }
    #endregion
}