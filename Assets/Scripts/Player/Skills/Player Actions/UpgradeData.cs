using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class UpgradeData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.UPGRADE;
    public override string name => "Upgrade";
    public override string description => $"Upgrades a structure.";
    public UpgradeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is Biolab biolab) {
            //TODO: Show Biolab Upgrade UI
        }
        base.ActivateAbility(structure);
    }
    #endregion
}