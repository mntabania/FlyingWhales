using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class EndRaidData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.END_RAID;
    public override string name { get { return "End Raid"; } }
    public override string description { get { return "End Raid"; } }

    public EndRaidData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            character.behaviourComponent.SetIsRaiding(false, null);
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        return true;
    }
    #endregion
}