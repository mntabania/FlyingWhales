using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;

public class DropItem : GoapAction {

    public override ACTION_CATEGORY actionCategory {
        get { return ACTION_CATEGORY.DIRECT; }
    }

    public DropItem() : base(INTERACTION_TYPE.DROP_ITEM) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, target = GOAP_EFFECT_TARGET.ACTOR }, IsItemInInventory);
    //}
    //public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
    //    List<Precondition> p = new List<Precondition>(base.GetPreconditions(actor, target, otherData));
    //    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, target.name, false, GOAP_EFFECT_TARGET.TARGET), IsItemInInventory));
    //    return p;
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        return otherData[0] as LocationStructure;
    }
    public override void OnActionStarted(ActualGoapNode node) {
        node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        return goapActionInvalidity;
    }
    #endregion

    #region Preconditions
    private bool IsItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem(poiTarget as TileObject);
    }
    #endregion

    #region State Effects
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        goapNode.actor.UncarryPOI(goapNode.poiTarget as TileObject);
        //if(goapNode.associatedJobType == JOB_TYPE.TAKE_ARTIFACT) {
        //    goapNode.actor.behaviourComponent.SetIsDefending(false, null);
        //}
    }
    #endregion

    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if(poiTarget is TileObject item) {
            if(actor.HasItem(poiTarget as TileObject)) {
                return false;
            }
        }
        if (actor.ownParty.IsPOICarried(poiTarget)) {
            return false;
        }
        if (poiTarget.IsAvailable() == false || poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
}
