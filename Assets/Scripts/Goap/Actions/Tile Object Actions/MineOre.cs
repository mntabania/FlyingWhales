
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class MineOre : GoapAction {

    public int m_amountProducedPerTick = 1;

    public MineOre() : base(INTERACTION_TYPE.MINE_ORE) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Mine Ore Success", goapNode);
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
        ProduceOrePile(node);
    }
	#endregion

	#region State Effects
    public void AfterMineOreSuccess(ActualGoapNode goapNode) {
        ProduceOrePile(goapNode);
    }
    #endregion

    void ProduceOrePile(ActualGoapNode p_node) {
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        
        int amount = p_node.currentStateDuration * m_amountProducedPerTick;
        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        stonePile.SetResourceInPile(amount);
        tileToSpawnPile.structure.AddPOI(stonePile, tileToSpawnPile);
        ProduceLogs(p_node);
        (p_node.target as TileObject).DestroyMapVisualGameObject();
        (p_node.target as TileObject).DestroyPermanently();
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, (p_node.currentStateDuration * m_amountProducedPerTick).ToString(), LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log);
    }
}