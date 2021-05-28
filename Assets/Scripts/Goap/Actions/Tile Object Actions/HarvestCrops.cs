
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class HarvestCrops : GoapAction {

    public int m_amountProducedPerTick = 1;

    public HarvestCrops() : base(INTERACTION_TYPE.HARVEST_CROPS) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Harvest Crops Success", goapNode);
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
        ProduceMatsPile(node);
    }
    #endregion

    #region State Effects
    public void AfterHarvestCropsSuccess(ActualGoapNode p_node) {
        p_node.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(ProduceMatsPile(p_node));
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        LocationGridTile tileToSpawnItem = p_node.actor.gridTileLocation;
        if (tileToSpawnItem != null && tileToSpawnItem.tileObjectComponent.objHere != null) {
            tileToSpawnItem = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        Summon summon = p_node.target as Summon;

        FoodPile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<FoodPile>((p_node.target as TileObject).tileObjectType);
        matsToHaul.SetResourceInPile(p_node.currentStateDuration * m_amountProducedPerTick);
        tileToSpawnItem.structure.AddPOI(matsToHaul, tileToSpawnItem);
        p_node.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(matsToHaul);

        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString() + " " + (p_node.target as TileObject).tileObjectType;
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log);
    }
}