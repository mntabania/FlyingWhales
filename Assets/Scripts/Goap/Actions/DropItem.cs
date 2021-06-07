using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

public class DropItem : GoapAction {

    public override ACTION_CATEGORY actionCategory {
        get { return ACTION_CATEGORY.DIRECT; }
    }

    public DropItem() : base(INTERACTION_TYPE.DROP_ITEM) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON, RACE.TROLL, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsItemInInventory);
    //}
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
        //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
        //p.AddRange(baseP);
        Precondition p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, target.name, false, GOAP_EFFECT_TARGET.TARGET), IsItemInInventory);
        isOverridden = true;
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        OtherData[] otherData = node.otherData;
        return otherData[0]?.obj as LocationStructure;
    }
    public override void OnActionStarted(ActualGoapNode node) {
        node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI(poiTarget, dropLocation: actor.gridTileLocation);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(poiTarget);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        return goapActionInvalidity;
    }
#endregion

#region Preconditions
    private bool IsItemInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return actor.HasItem(poiTarget as TileObject);
    }
#endregion

#region State Effects
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        //if current grid location is occupied and cannot get any unoccupied tile from current location, then just let the object disappear
        LocationGridTile tile = goapNode.actor.gridTileLocation;
        LocationStructure targetStructure = GetTargetStructure(goapNode);
        if(tile != null && tile.tileObjectComponent.objHere != null) {
            tile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: true);
            if (tile == null) {
                //in case no tile was found inside structure
                tile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();    
            }
        }
        if(targetStructure != null && tile.structure != targetStructure && targetStructure.passableTiles.Count > 0) {
            tile = CollectionUtilities.GetRandomElement(targetStructure.passableTiles);        
        }
        bool addToLocation = tile != null;
        goapNode.actor.UncarryPOI(goapNode.poiTarget as TileObject, addToLocation: addToLocation, dropLocation: tile);
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
        if (actor.carryComponent.IsPOICarried(poiTarget)) {
            return false;
        }
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
