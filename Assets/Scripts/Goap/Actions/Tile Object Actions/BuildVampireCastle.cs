using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class BuildVampireCastle : GoapAction {

    public BuildVampireCastle() : base(INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE) {
        actionIconString = GoapActionStateDB.Found_Icon;
        showNotification = true;
        //advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Work};
    }

    #region Overrides
    // public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData) {
    //     if(target is GenericTileObject genericTileObject) {
    //         if (genericTileObject.blueprintOnTile != null) {
    //             List<Precondition> p = new List<Precondition>();
    //             switch (genericTileObject.blueprintOnTile.thinWallResource) {
    //                 case RESOURCE.STONE:
    //                     p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
    //                     break;
    //                 case RESOURCE.WOOD:
    //                     p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
    //                     break;
    //                 case RESOURCE.METAL:
    //                     p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
    //                     break;
    //                 default:
    //                     p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
    //                     break;
    //             }
    //             return p;
    //         }
    //     }
    //     return base.GetPreconditions(actor, target, otherData);
    // }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    // public override void OnStopWhileStarted(ActualGoapNode node) {
    //     base.OnStopWhileStarted(node);
    //     Character actor = node.actor;
    //     actor.UncarryPOI();
    // }
    // public override void OnStopWhilePerforming(ActualGoapNode node) {
    //     base.OnStopWhilePerforming(node);
    //     Character actor = node.actor;
    //     IPointOfInterest poiTarget = node.poiTarget;
    //     actor.UncarryPOI();
    // }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity invalidity = base.IsInvalid(node);
        if (!invalidity.isInvalid) {
            if (node.poiTarget is GenericTileObject genericTileObject) {
                string prefabName = (string)node.otherData[0].obj;
                if (!LandmarkManager.Instance.HasEnoughSpaceForStructure(prefabName, genericTileObject.gridTileLocation)) {
                    invalidity.isInvalid = true;
                    invalidity.reason = "no_space_village";
                }    
            }
        }
        return invalidity;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget is GenericTileObject genericTileObject) {
                if (genericTileObject.blueprintOnTile != null) {
                    return false;
                }
                if (genericTileObject.gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                    return false;
                }
                // if (genericTileObject.numOfActionsBeingPerformedOnThis > 1) {
                //     return false; //this is to prevent multiple build actions on one tile, since it will cause overlap
                // }
            } else {
                return false;
            }
            return true;
        }
        return false;
    }
#endregion

    // #region Preconditions
    // private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
    //     if (poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null) {
    //         if (poiTarget.HasResourceAmount(genericTileObject.blueprintOnTile.thinWallResource, genericTileObject.blueprintOnTile.craftCost)) {
    //             return true;
    //         }
    //         //return actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile;
    //         if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is ResourcePile resourcePile) {
    //             return resourcePile.providedResource == genericTileObject.blueprintOnTile.thinWallResource && resourcePile.resourceInPile >= genericTileObject.blueprintOnTile.craftCost;
    //         }    
    //     }
    //     return false;
    // }
    // #endregion

#region State Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            string prefabName = (string)goapNode.otherData[0].obj;
            if (LandmarkManager.Instance.HasEnoughSpaceForStructure(prefabName, genericTileObject.gridTileLocation)) {
                NPCSettlement settlement = goapNode.actor.homeSettlement;
                bool createdNewSettlement = false;
                Area area = genericTileObject.gridTileLocation.area;
                //create new settlement if vampire has no home settlement yet
                if (goapNode.actor.homeSettlement == null) {
                    createdNewSettlement = true;
                    settlement = LandmarkManager.Instance.CreateNewSettlement(goapNode.actor.currentRegion, LOCATION_TYPE.VILLAGE);
                    LandmarkManager.Instance.OwnSettlement(goapNode.actor.faction, settlement);
                    settlement.SetSettlementType(LandmarkManager.Instance.GetSettlementTypeForCharacter(goapNode.actor));
                    // if (goapNode.actor.faction.race == RACE.HUMANS) {
                    //     settlement.SetSettlementType(SETTLEMENT_TYPE.Default_Human);
                    // } else if (goapNode.actor.faction.race == RACE.ELVES) {
                    //     settlement.SetSettlementType(SETTLEMENT_TYPE.Default_Elf);
                    // } else {
                    //     settlement.SetSettlementType(SETTLEMENT_TYPE.Default_Human);
                    // }
                    VillageSpot villageSpot = goapNode.actor.currentRegion.GetVillageSpotOnArea(area);
                    Assert.IsNotNull(villageSpot, $"New village {settlement} founded by {goapNode.actor.name} is being placed on area without a village spot! Area is {area}");
                    settlement.SetOccupiedVillageSpot(villageSpot);
                }
                
                settlement.AddAreaToSettlement(area);

                List<LocationStructure> createdStructures = new List<LocationStructure>();
                createdStructures.Add(LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(settlement, goapNode.actor.currentRegion.innerMap, genericTileObject.gridTileLocation, prefabName));

                if (createdNewSettlement) {
                    settlement.settlementJobTriggerComponent.KickstartJobs();
                }
                
                LocationStructure castle = createdStructures[0];
                goapNode.actor.MigrateHomeStructureTo(castle);
            }
        }
    }
#endregion
}

