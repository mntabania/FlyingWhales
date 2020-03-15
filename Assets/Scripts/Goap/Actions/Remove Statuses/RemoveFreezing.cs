using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RemoveFreezing : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public RemoveFreezing() : base(INTERACTION_TYPE.REMOVE_FREEZING) {
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Ember", target = GOAP_EFFECT_TARGET.ACTOR }, HasEmber);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Freezing", target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Frozen", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region State Effects
    public void AfterRemoveSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Remove Poisoned Trait from target table
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Freezing");
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Frozen");

        //**Effect 2**: Remove Tool from Actor's inventory
        TileObject ember = goapNode.actor.GetItem(TILE_OBJECT_TYPE.EMBER);
        if (ember != null) {
            goapNode.actor.UnobtainItem(ember);
        } else {
            //the actor does not have a tool, log for now
            goapNode.actor.logComponent.PrintLogErrorIfActive(
                $"{goapNode.actor.name} does not have a tool for removing poison! Poison was still removed, but thought you should know.");
        }
       
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            return poiTarget.traitContainer.HasTrait("Freezing", "Frozen");
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasEmber(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem(TILE_OBJECT_TYPE.EMBER);
    }
    #endregion
}