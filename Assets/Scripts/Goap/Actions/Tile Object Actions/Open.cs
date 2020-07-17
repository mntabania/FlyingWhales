using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;

public class Open  : GoapAction {

    public Open() : base(INTERACTION_TYPE.OPEN) {
        actionIconString = GoapActionStateDB.Inspect_Icon;
        
        advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        validTimeOfDays = new[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON };
    }

    #region Overrides
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    // }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Open Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
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
        if (treasureChest.objectInside is Mimic summon) {
            if (summon.marker == null) {
                treasureChest.SpawnInitialMimic(gridTileLocation, summon);
            } else {
                summon.marker.PlaceMarkerAt(gridTileLocation);
                summon.SetIsTreasureChest(false);
                TraitManager.Instance.CopyStatuses(treasureChest, summon);
            }
            
            goapNode.actor.currentStructure.RemovePOI(goapNode.poiTarget);
        } else {
            goapNode.actor.currentStructure.RemovePOI(goapNode.poiTarget);
            if (treasureChest.objectInside is ResourcePile resourcePile) {
                resourcePile.SetResourceInPile(50);
            }
            structure.AddPOI(treasureChest.objectInside, gridTileLocation);
        }
    }
    #endregion
}
