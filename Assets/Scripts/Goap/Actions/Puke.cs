﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Puke : GoapAction {

    public Puke() : base(INTERACTION_TYPE.PUKE) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Puke Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 5;
    }
    #endregion

    #region State Effects
    public void PrePukeSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.INACTIVE);
    }
    public void AfterPukeSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.ACTIVE);
        //if (recipient != null) {
        //    CreateRemoveTraitJob(recipient);
        //}
        //isPuking = false;
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