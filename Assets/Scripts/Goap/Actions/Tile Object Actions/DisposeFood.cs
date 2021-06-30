public class DisposeFood : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    public DisposeFood() : base(INTERACTION_TYPE.DISPOSE_FOOD) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }

    #region Overrides
    // public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
    //     Precondition p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, target.name, false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory);
    //     isOverridden = true;
    //     return p;
    // }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        return goapActionInvalidity;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Dispose Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    // public override void OnActionStarted(ActualGoapNode node) {
    //     node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
    // }
    // public override void OnStopWhileStarted(ActualGoapNode node) {
    //     base.OnStopWhileStarted(node);
    //     Character actor = node.actor;
    //     IPointOfInterest poiTarget = node.poiTarget;
    //     actor.UncarryPOI(poiTarget, dropLocation: actor.gridTileLocation);
    // }
    // public override void OnStopWhilePerforming(ActualGoapNode node) {
    //     base.OnStopWhilePerforming(node);
    //     Character actor = node.actor;
    //     IPointOfInterest poiTarget = node.poiTarget;
    //     actor.UncarryPOI(poiTarget);
    // }
#endregion

// #region Preconditions
//     private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
//         return actor.IsPOICarriedOrInInventory(poiTarget);
//     }
// #endregion
    
#region Effects
    public void AfterDisposeSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is FoodPile foodPile) {
            // goapNode.actor.UncarryPOI(foodPile, addToLocation: true);
            foodPile.AdjustHP(-foodPile.maxHP, ELEMENTAL_TYPE.Normal);
        }
    }
#endregion
    
    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        // if(poiTarget is TileObject item) {
        //     if(actor.HasItem(poiTarget as TileObject)) {
        //         return false;
        //     }
        // }
        // if (actor.carryComponent.IsPOICarried(poiTarget)) {
        //     return false;
        // }
        if (poiTarget.IsAvailable() == false || poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, true) == false) {
                if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget)) {
                    return false;
                }
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile, true) == false) {
                return true;
            }
        }
        return false;
    }
}
