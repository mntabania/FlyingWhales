using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class StopData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.STOP;
    public override string name { get { return "Stop"; } }
    public override string description { get { return "Stop"; } }

    public StopData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            character.jobComponent.TriggerStopJobs();
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        return true;
    }
    #endregion
}