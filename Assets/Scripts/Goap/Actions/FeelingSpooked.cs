using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class FeelingSpooked : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public FeelingSpooked() : base(INTERACTION_TYPE.FEELING_SPOOKED) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget });
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Spooked Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 10;
    }
    #endregion
}

public class FeelingSpookedData : GoapActionData {
    public FeelingSpookedData() : base(INTERACTION_TYPE.FEELING_SPOOKED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}