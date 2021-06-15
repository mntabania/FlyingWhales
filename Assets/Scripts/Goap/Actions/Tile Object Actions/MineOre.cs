
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;

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
        if (node.currentStateDuration > 0) {
            ProduceMatsPile(node);
        }
    }
	#endregion

	#region State Effects
    public void AfterMineOreSuccess(ActualGoapNode p_node) {
        Ore targetOre = p_node.target as Ore;
        Assert.IsNotNull(targetOre);
        if (targetOre.gridTileLocation != null) {
            targetOre.gridTileLocation.structure.RemovePOI(targetOre);    
        }
        p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(ProduceMatsPile(p_node));
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        Ore targetOre = p_node.target as Ore;
        Assert.IsNotNull(targetOre);
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        MetalPile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(targetOre.providedMetal.ConvertResourcesToTileObjectType());
        matsToHaul.SetResourceInPile(p_node.currentStateDuration * m_amountProducedPerTick);
        tileToSpawnPile.structure.AddPOI(matsToHaul, tileToSpawnPile);
        ProduceLogs(p_node);
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(4, p_node.actor);
        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString();
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log);
    }
}