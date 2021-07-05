using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class BuildNewVillage : GoapAction {

    public BuildNewVillage() : base(INTERACTION_TYPE.BUILD_NEW_VILLAGE) {
        actionIconString = GoapActionStateDB.Found_Icon;
        showNotification = true;
        //advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Major};
    }

    #region Overrides
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

#region State Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            string prefabName = (string)goapNode.otherData[0].obj;
            if (LandmarkManager.Instance.HasEnoughSpaceForStructure(prefabName, genericTileObject.gridTileLocation)) {
                NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(goapNode.actor.currentRegion, LOCATION_TYPE.VILLAGE);
                if(goapNode.actor.faction != null && goapNode.actor.faction.isMajorNonPlayer) {
                    LandmarkManager.Instance.OwnSettlement(goapNode.actor.faction, settlement);
                }

                Area area = genericTileObject.gridTileLocation.area;
                settlement.AddAreaToSettlement(area);
                VillageSpot villageSpot = goapNode.actor.currentRegion.GetVillageSpotOnArea(area);
                Assert.IsNotNull(villageSpot, $"New village {settlement} founded by {goapNode.actor.name} is being placed on area without a village spot! Area is {area}");
                settlement.SetOccupiedVillageSpot(villageSpot);
                
                List<LocationStructure> createdStructures = new List<LocationStructure>();
                createdStructures.Add(LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(settlement, goapNode.actor.currentRegion.innerMap, genericTileObject.gridTileLocation, prefabName));

                settlement.PlaceInitialObjects();
                settlement.settlementJobTriggerComponent.KickstartJobs();
                
                LocationStructure firstStructure = createdStructures[0];
                goapNode.actor.MigrateHomeStructureTo(firstStructure);
                
                //added this after migrating home of character since it will trigger NPCSettlement.ChangeSettlementTypeAccordingTo which will overwrite any changes we make to the settlement type.
                settlement.SetSettlementType(LandmarkManager.Instance.GetSettlementTypeForCharacter(goapNode.actor));
                
                // if (goapNode.actor.faction.race == RACE.HUMANS) {
                //     settlement.SetSettlementType(SETTLEMENT_TYPE.Default_Human);
                // } else if (goapNode.actor.faction.race == RACE.ELVES) {
                //     settlement.SetSettlementType(SETTLEMENT_TYPE.Default_Elf);
                // } else {
                //     settlement.SetSettlementType(SETTLEMENT_TYPE.Default_Human);
                // }

                //This is added since the character will be the first character in the settlement, it should learn how to build structures, so that when he place blueprints to build houses, etc, he can also build them
                //If we do not add this, the character will just place blueprints and will not build them if his class does not know how to build, so he will end up waiting for another character to join the settlement that can build structures
                goapNode.actor.jobComponent.AddAbleJob(JOB_TYPE.BUILD_BLUEPRINT);
            }
        }
    }
#endregion
}

