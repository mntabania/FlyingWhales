using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RemoveUnconscious : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public RemoveUnconscious() : base(INTERACTION_TYPE.REMOVE_UNCONSCIOUS) {
        actionIconString = GoapActionStateDB.Cure_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Water Flask", target = GOAP_EFFECT_TARGET.ACTOR }, HasWaterFlask);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
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
    //public void PreRemovePoisonSuccess(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure.location, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterRemoveSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Remove Poisoned Trait from target table
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Unconscious");
        //**Effect 2**: Remove Tool from Actor's inventory
        TileObject tool = goapNode.actor.GetItem(TILE_OBJECT_TYPE.WATER_FLASK);
        if (tool != null) {
            goapNode.actor.UnobtainItem(tool);
        } else {
            //the actor does not have a tool, log for now
            goapNode.actor.logComponent.PrintLogErrorIfActive(
                $"{goapNode.actor.name} does not have a tool for removing unconscious! Unconscious was still removed, but thought you should know.");
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
            return poiTarget.traitContainer.HasTrait("Unconscious");
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasWaterFlask(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem(TILE_OBJECT_TYPE.WATER_FLASK);
    }
    #endregion
}