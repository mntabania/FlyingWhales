﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class FindFish : GoapAction {

    public int m_amountProducedPerTick = 1;

    public FindFish() : base(INTERACTION_TYPE.FIND_FISH) {
        actionIconString = GoapActionStateDB.Fish_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Find Fish Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);

        return satisfied;
    }

    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.currentStateDuration > 0) {
            ProduceMatsPile(node);
        }
    }
    #endregion

    #region State Effects
    public void AfterFindFishSuccess(ActualGoapNode p_node) {
        p_node.actor.jobComponent.TryCreateHaulJob(ProduceMatsPile(p_node));
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        FishPile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<FishPile>(TILE_OBJECT_TYPE.FISH_PILE);
        matsToHaul.SetResourceInPile(p_node.currentStateDuration * m_amountProducedPerTick);
        tileToSpawnPile.structure.AddPOI(matsToHaul, tileToSpawnPile);
        ProduceLogs(p_node);

        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, (p_node.currentStateDuration * m_amountProducedPerTick).ToString(), LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log);
    }
}