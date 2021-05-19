using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using UnityEngine;  
using Traits;
using UtilityScripts;
using Locations.Settlements;
using UnityEngine.Assertions;

public class HarvestPlant : GoapAction {

    public HarvestPlant() : base(INTERACTION_TYPE.HARVEST_PLANT) {
        actionIconString = GoapActionStateDB.Harvest_Icon;
        
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Harvest Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
#if DEBUG_LOG
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
#endif
            return 2000;
        }
        if(target.gridTileLocation != null) {
            BaseSettlement settlement;
            if(target.gridTileLocation.IsPartOfSettlement(out settlement)) {
                if(settlement.owner != null && actor.homeSettlement != settlement) {
                    //If target is in a claimed settlement and actor's home settlement is not the target's settlement, do not harvest, even if the faction owner of the target's settlement is also the faciton of the actor
#if DEBUG_LOG
                    costLog += $" +2000(Target's settlement is not the actor's home settlement)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                }
            }
        }
        if(job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                LocationGridTile centerGridTileOfActor = actor.areaLocation.gridTileComponent.centerGridTile;
                float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                if(distance > distanceToCheck) {
                    //target is at structure that character is avoiding
#if DEBUG_LOG
                    costLog += $" +2000(Location of target too far from actor)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(40, 51);
#if DEBUG_LOG
        costLog += $" +{cost.ToString()}(Random Cost Between 40-50)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(null, GetTargetString(node.poiTarget), LOG_IDENTIFIER.STRING_2);
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
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is Crops crops && crops.currentGrowthState != Crops.Growth_State.Ripe) {
                return false;
            }
            return poiTarget.IsAvailable() &&
                   poiTarget.gridTileLocation != null; //&& actor.traitContainer.HasTrait("Worker");
        }
        return false;
    }
#endregion

#region State Effects
    public void PreHarvestSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(null, "30", LOG_IDENTIFIER.STRING_1);
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        //}
    }
    public void PerTickHarvestSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustHappiness(-4);
        }
    }
    public void AfterHarvestSuccess(ActualGoapNode goapNode) {
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //}
        IPointOfInterest poiTarget = goapNode.poiTarget;
        Assert.IsTrue(poiTarget is Crops);
        if (poiTarget is Crops crop) {
            crop.SetGrowthState(Crops.Growth_State.Growing);
            
            List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
            poiTarget.gridTileLocation.PopulateTilesInRadius(choices, 1, includeTilesInDifferentStructure: true, includeImpassable: false);
            if (choices.Count > 0) {
                FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, CollectionUtilities.GetRandomElement(choices));
                if(goapNode.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
                    if(goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.targetCamp != null) {
                        goapNode.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, goapNode.actor.partyComponent.currentParty.targetCamp);
                        goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
                    }
                } else {
                    if (foodPile != null && goapNode.actor.homeSettlement != null) {
                        goapNode.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(foodPile);
                        goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
                    }
                }
            }
            RuinarchListPool<LocationGridTile>.Release(choices);
        } 
        // else {
        //     LocationGridTile tile = poiTarget.gridTileLocation;
        //     tile.structure.RemovePOI(poiTarget);
        //     
        //     FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.VEGETABLES);
        //     foodPile.SetResourceInPile(30);
        //     tile.structure.AddPOI(foodPile, tile);
        //     if (foodPile != null && goapNode.actor.homeSettlement != null) {
        //         goapNode.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(foodPile);
        //         goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
        //     }
        // }
    }
#endregion

#region Utilities
    private string GetTargetString(IPointOfInterest poi) {
        if (poi is BerryShrub) {
            return "berries";
        } else if (poi is CornCrop) {
            return "corn";
        } else if (poi is Mushroom) {
            return "mushrooms";
        } else {
            return poi.name;
        }
    }
#endregion
}