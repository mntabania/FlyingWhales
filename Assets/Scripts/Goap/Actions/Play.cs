﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Play : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Play() : base(INTERACTION_TYPE.PLAY) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT, };
        actionIconString = GoapActionStateDB.Happy_Icon;
        
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Play Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        //**Cost**: randomize between 6-15
        return UtilityScripts.Utilities.Rng.Next(6, 16);
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    #endregion

    #region Effects
    //public void PrePlaySuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
    //}
    public void PerTickPlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(6f);
    }
    //public void AfterPlaySuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}