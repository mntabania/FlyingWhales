using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class SepticShock : GoapAction {

    public SepticShock() : base(INTERACTION_TYPE.SEPTIC_SHOCK) {
        actionIconString = GoapActionStateDB.Death_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Septic Shock Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 5;
    }
    #endregion

    #region State Effects
    public void PreSepticShockSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.INACTIVE);
    }
    public void AfterSepticShockSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.ACTIVE);
        goapNode.actor.Death("Septic Shock", goapNode, _deathLog: goapNode.descriptionLog);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}