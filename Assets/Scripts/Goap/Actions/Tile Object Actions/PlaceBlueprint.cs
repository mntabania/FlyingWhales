using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBlueprint : GoapAction {

    public PlaceBlueprint() : base(INTERACTION_TYPE.PLACE_BLUEPRINT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Place Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 3;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(log, goapNode);
        STRUCTURE_TYPE structureType = (STRUCTURE_TYPE)goapNode.otherData[0];
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            BuildSpotTileObject buildSpot = poiTarget as BuildSpotTileObject;
            return buildSpot.spot.IsOpenFor(actor.homeSettlement);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PrePlaceSuccess(ActualGoapNode goapNode) {
        STRUCTURE_TYPE structureType = (STRUCTURE_TYPE)goapNode.otherData[0];
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterPlaceSuccess(ActualGoapNode goapNode) {
        STRUCTURE_TYPE structureType = (STRUCTURE_TYPE)goapNode.otherData[0];
        BuildSpotTileObject spot = goapNode.poiTarget as BuildSpotTileObject;
        // spot.PlaceBlueprintOnBuildingSpot(structureType);


        //create new build job at npcSettlement
        // GoapPlanJob buildJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_BLUEPRINT, INTERACTION_TYPE.BUILD_STRUCTURE, spot, goapNode.actor.homeSettlement);
        // buildJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 50 });
        // buildJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeBuildJob);
        // goapNode.actor.homeSettlement.AddToAvailableJobs(buildJob);

        goapNode.actor.buildStructureComponent.OnCreateBlueprint(structureType);
    }
    #endregion
}
