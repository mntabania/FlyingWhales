using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete("This is no longer used because structures can now be cast independently.")]
public class BuildDemonicStructureData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BUILD_DEMONIC_STRUCTURE;
    public override string name => "Build Structure";
    public override string description => "Build a Demonic Structure";
    public BuildDemonicStructureData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.HEX };
    }
    #region Overrides
    public override void ActivateAbility(Area targetArea) { }
    public override bool IsValid(IPlayerActionTarget target) {
        return false;
    }
    #endregion
}