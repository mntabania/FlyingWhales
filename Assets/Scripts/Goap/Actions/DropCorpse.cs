using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
using UtilityScripts;
using Locations.Settlements;

public class DropCorpse : GoapAction {

    public DropCorpse() : base(INTERACTION_TYPE.DROP_CORPSE) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Haul_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        logTags = new[] {LOG_TAG.Work};
    }

    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Corpse", false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory);
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Drop Success", actionNode);
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
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0].obj is LocationStructure) {
                return otherData[0].obj as LocationStructure;
            } else if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile) {
                return otherData[0].obj as LocationStructure;
            }
        }
        return base.GetTargetStructure(node);
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        if (otherData != null) {
            if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile) {
                return otherData[1].obj as LocationGridTile;
            }
        }
        return null;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI(poiTarget);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI(poiTarget);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsDropTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        return goapActionInvalidity;
    }
    private bool IsDropTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.IsAvailable() == false 
            || (poiTarget.gridTileLocation == null && node.actor.IsPOICarriedOrInInventory(poiTarget) == false)) {
            return true;
        }
        return false;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (actor == poiTarget) {
                return false;
            }
            if (otherData != null) {
                if (otherData.Length == 1 && otherData[0].obj is LocationStructure structure) {
                    return actor.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(structure.passableTiles));
                } else if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile targetTile) {
                    return actor.movementComponent.HasPathToEvenIfDiffRegion(targetTile);
                }
            }
            return true;
        }
        return false;
    }
#endregion

#region Preconditions
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        // if (poiTarget is Character) {
        //     Character target = poiTarget as Character;
        //     return target.currentParty == actor.currentParty;    
        // } else {
        //     return actor.ownParty.IsPOICarried(poiTarget);
        // }
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
#endregion

#region State Effects
    //public void PreDropSuccess(ActualGoapNode goapNode) {
    //    //GoapActionState currentState = this.states[goapNode.currentStateName];
    //    goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        //Character target = goapNode.poiTarget as Character;
        OtherData[] otherData = goapNode.otherData;
        LocationGridTile tile = null;
        if (otherData != null) {
            if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile) {
                tile = otherData[1].obj as LocationGridTile;
            }
        }
        goapNode.actor.UncarryPOI(goapNode.poiTarget, dropLocation: tile);
    }
#endregion
}