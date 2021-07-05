using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RemoveUnconscious : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    public RemoveUnconscious() : base(INTERACTION_TYPE.REMOVE_UNCONSCIOUS) {
        actionIconString = GoapActionStateDB.Cure_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        // AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Water Flask", target = GOAP_EFFECT_TARGET.ACTOR }, HasWaterFlask);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
#if DEBUG_LOG
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
#endif
            return 2000;
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region State Effects
    //public void PreRemovePoisonSuccess(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure.location, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterRemoveSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.moneyComponent.AdjustCoins(10);
        //**Effect 1**: Remove Poisoned Trait from target table
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Unconscious");
        //**Effect 2**: Remove Tool from Actor's inventory
        TileObject item = goapNode.actor.GetItem(TILE_OBJECT_TYPE.WATER_FLASK);
        if (item != null) {
            goapNode.actor.UnobtainItem(item);
        }
    }
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
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