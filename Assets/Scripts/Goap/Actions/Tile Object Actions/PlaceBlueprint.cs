using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;
using UtilityScripts;

public class PlaceBlueprint : GoapAction {

    public PlaceBlueprint() : base(INTERACTION_TYPE.PLACE_BLUEPRINT) {
        actionIconString = GoapActionStateDB.Blueprint_Icon;
        showNotification = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Place Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 3;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(log, goapNode);
        StructureSetting structureSetting = (StructureSetting)goapNode.otherData[2].obj;
        log.AddToFillers(null, structureSetting.structureType.StructureName(), LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (!actor.isSettlementRuler) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PrePlaceSuccess(ActualGoapNode goapNode) {
        string prefabName = (string)goapNode.otherData[0].obj;
        LocationGridTile connectorTile = (LocationGridTile)goapNode.otherData[1].obj;
        StructureSetting structureSetting = (StructureSetting)goapNode.otherData[2].obj;
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            bool successfullyPlacedBlueprint = false;
            if (!LandmarkManager.Instance.HasAffectedCorruptedTilesForStructure(prefabName, genericTileObject.gridTileLocation)) {
                if (genericTileObject.PlaceExpiringBlueprintOnTile(prefabName)) {
                    successfullyPlacedBlueprint = true;
                    //create new build job at npcSettlement
                    NPCSettlement settlement = goapNode.actor.homeSettlement;
                    if (settlement != null) {
                        GoapPlanJob buildJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_BLUEPRINT, INTERACTION_TYPE.BUILD_BLUEPRINT, goapNode.poiTarget, settlement);
                        buildJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { genericTileObject.blueprintOnTile.craftCost });
                        buildJob.AddOtherData(INTERACTION_TYPE.BUILD_BLUEPRINT, new object[] { connectorTile });
                        JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(settlement, buildJob, INTERACTION_TYPE.TAKE_RESOURCE);
                        List<LocationStructure> mines = settlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
                        if (mines != null) {
                            for (int i = 0; i < mines.Count; i++) {
                                LocationStructure mine = mines[i];
                                buildJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, mine);
                            }    
                        }
                        List<LocationStructure> lumberyards = settlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
                        if (lumberyards != null) {
                            for (int i = 0; i < lumberyards.Count; i++) {
                                LocationStructure lumberyard = lumberyards[i];
                                buildJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, lumberyard);
                            }    
                        }
                        // buildJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeBuildJob);
                        settlement.AddToAvailableJobs(buildJob);
                    }
                    goapNode.descriptionLog.AddToFillers(null, structureSetting.structureType.StructureName(), LOG_IDENTIFIER.STRING_1);
                }
            }
            if (!successfullyPlacedBlueprint) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "fail", goapNode, LOG_TAG.Work);
                log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, structureSetting.structureType.StructureName(), LOG_IDENTIFIER.STRING_1);
                goapNode.OverrideDescriptionLog(log);
            }
        }
    }
    #endregion
}
