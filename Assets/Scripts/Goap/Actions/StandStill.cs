using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class StandStill : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public StandStill() : base(INTERACTION_TYPE.STAND_STILL) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.No_Icon;
        showNotification = false;
        
        shouldAddLogs = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.GOLEM, RACE.DEMON, RACE.WOLF, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ENT };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Stand Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 4;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}
