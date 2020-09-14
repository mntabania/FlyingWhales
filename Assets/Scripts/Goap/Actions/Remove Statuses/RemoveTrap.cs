﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RemoveTrap : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;
    public RemoveTrap() : base(INTERACTION_TYPE.REMOVE_TRAP) {
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Booby Trapped", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = "";
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
            return 2000;
        }
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region State Effects
    public void AfterRemoveSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Remove Booby Trapped Trait from target table
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Booby Trapped");
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            return poiTarget.traitContainer.HasTrait("Booby Trapped");
        }
        return false;
    }
    #endregion
}