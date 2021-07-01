using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class CreateHospiceAntidote : GoapAction {

    public CreateHospiceAntidote() : base(INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides

    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Create Hospice Antidote Success", goapNode);
    }
    //public override void AddFillersToLog(Log log, ActualGoapNode node) {
    //    base.AddFillersToLog(log, node);
    //    TileObject obj = node.poiTarget as TileObject;
    //    log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
    //}
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = 10;
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    #endregion

    #region State Effects
    public void AfterCreateHospiceAntidoteSuccess(ActualGoapNode p_node) {
        p_node.actor.moneyComponent.AdjustCoins(28);
        TileObject antidote = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE);
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        
        tileToSpawnPile.structure.AddPOI(antidote, tileToSpawnPile);
        p_node.actor.structureComponent.workPlaceStructure.RemovePOI(p_node.target);
        p_node.actor.jobComponent.CreateDropItemJob(JOB_TYPE.CREATE_HOSPICE_ANTIDOTE, antidote, p_node.actor.structureComponent.workPlaceStructure);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        return satisfied;
    }
    #endregion
}