using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ScreamForHelp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public ScreamForHelp() : base(INTERACTION_TYPE.SCREAM_FOR_HELP) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.MAKE_NOISE, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Scream Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    #endregion

    #region State Effects
    public void PerTickScreamSuccess(ActualGoapNode goapNode) {
        Messenger.Broadcast(Signals.SCREAM_FOR_HELP, goapNode.actor);
    }
    public void AfterScreamSuccess(ActualGoapNode goapNode) {

    }
    #endregion
}

public class ScreamForHelpData : GoapActionData {
    public ScreamForHelpData() : base(INTERACTION_TYPE.SCREAM_FOR_HELP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
