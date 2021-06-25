using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;
using Traits;

public class DropResourceToWorkStructure : GoapAction {
    public DropResourceToWorkStructure() : base(INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    // }
    
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        Precondition p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, target.name, false, GOAP_EFFECT_TARGET.ACTOR), IsCarriedOrInInventory);
        isOverridden = true;
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Resource To Work Structure Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        if (actor.carryComponent.carriedPOI is ResourcePile resourcePile) {
            actor.UncarryPOI(resourcePile, dropLocation: actor.gridTileLocation);
        }
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        if (actor.carryComponent.carriedPOI is ResourcePile resourcePile) {
            actor.UncarryPOI(resourcePile, dropLocation: actor.gridTileLocation);
        }
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(log, goapNode);
        if (goapNode.actor.carryComponent.carriedPOI is ResourcePile pile) {
            log.AddToFillers(null, pile.resourceInPile.ToString(), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(pile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);    
        }
    }
    public override void OnActionStarted(ActualGoapNode node) {
        node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
    }
    #endregion

    #region Preconditions
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(tileObject.tileObjectType) is ResourcePile pile && pile.resourceInPile >= 40;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (actor.IsPOICarriedOrInInventory(poiTarget)) {
                return true;
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation.IsPartOfSettlement()) {
                if (actor.homeSettlement != null && actor.homeSettlement.mainStorage.unoccupiedTiles.Count <= 0) {
                    return false;
                }
            }
            return actor.homeRegion == poiTarget.gridTileLocation.parentMap.region;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterDropResourceToWorkStructureSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        ResourcePile poiTarget = goapNode.poiTarget as ResourcePile;
        OtherData[] otherData = goapNode.otherData;
        if (poiTarget is ResourcePile pileToBeDepositedTo) {
            if (pileToBeDepositedTo.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
                //remove unbuilt pile, since it is no longer needed, then place carried pile in its place
                pileToBeDepositedTo.gridTileLocation.structure.RemovePOI(pileToBeDepositedTo);
                if (actor.carryComponent.isCarryingAnyPOI) {
                    actor.UncarryPOI(actor.carryComponent.carriedPOI, dropLocation: goapNode.targetTile);
                } else {
                    TileObject tileObjectInInventory = actor.GetItem(pileToBeDepositedTo.tileObjectType);
                    if (tileObjectInInventory != null) {
                        actor.DropItem(tileObjectInInventory, goapNode.targetTile);    
                    }
                }
            } else {
                //Deposit resource pile
                pileToBeDepositedTo.AdjustResourceInPile(poiTarget.resourceInPile);
                TraitManager.Instance.CopyStatuses(poiTarget, pileToBeDepositedTo);
                if (actor.carryComponent.isCarryingAnyPOI) {
                    actor.UncarryPOI(actor.carryComponent.carriedPOI, dropLocation: goapNode.targetTile);
                } else {
                    actor.UnobtainItem(poiTarget);
                }
            }    
        }
    }
    #endregion


    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (actor.carryComponent.IsPOICarried(poiTarget)) {
            return false;
        }
        if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion) {
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
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
}
