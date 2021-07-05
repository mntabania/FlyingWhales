using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using Traits;
using UnityEngine;

public class Open  : GoapAction {

    public Open() : base(INTERACTION_TYPE.OPEN) {
        actionIconString = GoapActionStateDB.Inspect_Icon;
        //advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    // }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Open Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        if (node.poiTarget is TreasureChest treasureChest && treasureChest.objectInside != null) {
            log.AddToFillers(treasureChest.objectInside, treasureChest.objectInside.name, LOG_IDENTIFIER.CHARACTER_3);
        }
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effetcs
    public void AfterOpenSuccess(ActualGoapNode goapNode) {
        TreasureChest treasureChest = goapNode.poiTarget as TreasureChest;
        LocationStructure structure = treasureChest.gridTileLocation.structure;
        LocationGridTile gridTileLocation = treasureChest.gridTileLocation;
        if (treasureChest.objectInside is Mimic mimic) {
            if (mimic.marker == null) {
                treasureChest.SpawnInitialMimic(gridTileLocation, mimic);
            } else {
                mimic.SetIsTreasureChest(false);
                mimic.marker.PlaceMarkerAt(gridTileLocation);
                mimic.marker.SetVisualState(true);
                TraitManager.Instance.CopyStatuses(treasureChest, mimic);
            }
            structure.RemovePOI(goapNode.poiTarget);
            mimic.UnsubscribeToAwakenMimicEvent(treasureChest);
            if (mimic.hasMarker) {
                //this is so that mimic will react to actor again, since it probably already saw him/her before it was opened
                mimic.marker.AddUnprocessedPOI(goapNode.actor);    
            }
        } else {
            structure.RemovePOI(goapNode.poiTarget);
            if (treasureChest.objectInside is ResourcePile resourcePile) {
                resourcePile.SetResourceInPile(50);
            }
            structure.AddPOI(treasureChest.objectInside, gridTileLocation);
        }
    }
#endregion
}
