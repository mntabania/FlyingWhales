using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Fish : GoapAction {

    public Fish() : base(INTERACTION_TYPE.FISH) {
        actionIconString = GoapActionStateDB.Fish_Icon;
        //advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        //racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        // validTimeOfDays = new[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON };
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
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                LocationGridTile centerGridTileOfActor = actor.gridTileLocation.area.gridTileComponent.centerGridTile;
                float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                if (distance > distanceToCheck) {
                    //target is at structure that character is avoiding
#if DEBUG_LOG
                    costLog += $" +2000(Location of target too far from actor)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 101);
#if DEBUG_LOG
        costLog += $" +{cost.ToString()}(Random Cost Between 80-100)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    if (node.actor.characterClass.IsCombatant()) {
    //        node.actor.needsComponent.AdjustDoNotGetBored(-1);
    //    }
    //}
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        return goapActionInvalidity;
    }
    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        //Action is invalid if the target is unavailable and the action cannot be advertised if target is unavailable
        if ((poiTarget.IsAvailable() == false && !canBeAdvertisedEvenIfTargetIsUnavailable) || poiTarget.gridTileLocation == null) {
            return true;
        }
        if (actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE && actor.currentRegion != poiTarget.gridTileLocation.structure.region) {
            return true;
        }
        LocationGridTile targetTile = poiTarget.gridTileLocation;
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_OTHER_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile, true) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEARBY || actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION
            || actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B || actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE) {
            //if the action type is NEARBY, RANDOM_LOCATION, RANDOM_LOCATION_B, OVERRIDE, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile, true) == false) {
                return true;
            }
        }
        return false;
    }
    #endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if(poiTarget is FishingSpot fishingSpot) {
                return actor.homeSettlement != null && fishingSpot.connectedFishingShack != null && fishingSpot.connectedFishingShack.settlementLocation == actor.homeSettlement
                    && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
            }
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effects
    public void PreFishSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(null, "50", LOG_IDENTIFIER.STRING_1);
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        //}
    }
    public void PerTickFishSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustHappiness(-4);
        }
    }
    public void AfterFishSuccess(ActualGoapNode goapNode) {
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //}
        LocationGridTile tile = goapNode.actor.gridTileLocation;
        if(tile != null && tile.tileObjectComponent.objHere != null) {
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
        goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(8, goapNode.actor);
    }
#endregion
}