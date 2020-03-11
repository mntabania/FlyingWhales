using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class GoTo : GoapAction {

    public GoTo() : base(INTERACTION_TYPE.GO_TO) {
        actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
        actionIconString = GoapActionStateDB.Work_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET ));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Goto Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    //#region State Effects
    //public void AfterSpookedSuccess() {
    //    //if (parentPlan != null && parentPlan.job != null) {
    //    //    parentPlan.job.SetCannotOverrideJob(true);//Carry should not be overrideable if the character is actually already carrying another character.
    //    //}
    //    Character target = poiTarget as Character;
    //    actor.ownParty.AddCharacter(target);
    //}
    //#endregion
}

public class GoToData : GoapActionData {
    public GoToData() : base(INTERACTION_TYPE.GO_TO) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}