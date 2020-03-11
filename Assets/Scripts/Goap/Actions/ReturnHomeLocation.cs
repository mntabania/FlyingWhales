using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class ReturnHomeLocation : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ReturnHomeLocation() : base(INTERACTION_TYPE.RETURN_HOME_LOCATION) {
        this.goapName = "Return Home Location";
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Return Home Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 3;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        return actor.homeRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
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

public class ReturnHomeLocationData : GoapActionData {
    public ReturnHomeLocationData() : base(INTERACTION_TYPE.RETURN_HOME_LOCATION) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
