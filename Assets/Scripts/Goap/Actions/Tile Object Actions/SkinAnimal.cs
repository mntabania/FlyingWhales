﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class SkinAnimal : GoapAction {

    public int m_amountProducedPerTick = 1;
    public SkinAnimal() : base(INTERACTION_TYPE.SKIN_ANIMAL) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Skin Animal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }

    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        ProduceMatsPile(node);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);

        return satisfied;
    }
    #endregion

    #region State Effects
    public void AfterSkinAnimalSuccess(ActualGoapNode p_node) {
        p_node.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(ProduceMatsPile(p_node));
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        LocationGridTile tileToSpawnItem = p_node.actor.gridTileLocation;
        if (tileToSpawnItem != null && tileToSpawnItem.tileObjectComponent.objHere != null) {
            tileToSpawnItem = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        ResourcePile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>((p_node.target as Summon).produceableMaterial);
        matsToHaul.SetResourceInPile(p_node.currentStateDuration * m_amountProducedPerTick);
        tileToSpawnItem.structure.AddPOI(matsToHaul, tileToSpawnItem);
        p_node.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(matsToHaul);

        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString() + " " + (p_node.target as Summon).produceableMaterial;
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log);
    }
}