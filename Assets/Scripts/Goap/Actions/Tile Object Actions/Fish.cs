using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Fish : GoapAction {

    public Fish() : base(INTERACTION_TYPE.FISH) {
        actionIconString = GoapActionStateDB.Fish_Icon;
        advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        validTimeOfDays = new[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Fish Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (target.gridTileLocation != null && target.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.gridTileLocation != null
                && actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                LocationGridTile centerGridTileOfTarget = target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                LocationGridTile centerGridTileOfActor = actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                int distanceToCheck = (InnerMapManager.BuildingSpotSize.x * 2) * 3;

                if (distance > distanceToCheck) {
                    //target is at structure that character is avoiding
                    costLog += $" +2000(Location of target too far from actor)";
                    actor.logComponent.AppendCostLog(costLog);
                    return 2000;
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 101); 
        costLog += $" +{cost.ToString()}(Random Cost Between 80-100)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.actor.characterClass.IsCombatant()) {
            node.actor.needsComponent.AdjustDoNotGetBored(-1);
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreFishSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(null, "50", LOG_IDENTIFIER.STRING_1);
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        }
    }
    public void PerTickFishSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustHappiness(-2);
        }
    }
    public void AfterFishSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        }
        LocationGridTile tile = goapNode.actor.gridTileLocation;
        if(tile != null && tile.objHere != null) {
            tile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }

        // FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FISH_PILE);
        // foodPile.SetResourceInPile(50);
        // tile.structure.AddPOI(foodPile, tile);
        // foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
        if(tile != null) {
            if (goapNode.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
                FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FISH_PILE);
                foodPile.SetResourceInPile(50);
                tile.structure.AddPOI(foodPile, tile);
                if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.targetCamp != null) {
                    goapNode.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, goapNode.actor.partyComponent.currentParty.targetCamp);
                    goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
                }
            } else {
                InnerMapManager.Instance.CreateNewResourcePileAndTryCreateHaulJob<FoodPile>(TILE_OBJECT_TYPE.FISH_PILE, 50, goapNode.actor, tile);
            }
        }
    }
    #endregion
}