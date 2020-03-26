using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class ReturnToPortalData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.RETURN_TO_PORTAL;
    public override string name { get { return "Return To Portal"; } }
    public override string description { get { return "Return To Portal"; } }

    public ReturnToPortalData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            character.jobComponent.TriggerReturnPortal();
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}