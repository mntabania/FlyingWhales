﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ScreamForHelp : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;
    public ScreamForHelp() : base(INTERACTION_TYPE.SCREAM_FOR_HELP) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Shock_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.MAKE_NOISE, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Scream Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    #endregion

    #region State Effects
    public void PerTickScreamSuccess(ActualGoapNode goapNode) {
        Messenger.Broadcast(JobSignals.SCREAM_FOR_HELP, goapNode.actor);
    }
    public void AfterScreamSuccess(ActualGoapNode goapNode) {

    }
    #endregion
}