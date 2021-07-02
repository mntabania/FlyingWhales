
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;

public class MineOre : GoapAction {

    public int m_amountProducedPerTick = 4;
    private const float _coinGainMultiplier = 0.330f;
    public MineOre() : base(INTERACTION_TYPE.MINE_ORE) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
        shouldAddLogs = false;
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
        ResourcePile pile = ProduceMatsPile(p_node);
        if (pile != null && pile.resourceInPile > 0) {
            p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(ProduceMatsPile(p_node));
        }
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        TileObject targetOre = p_node.target as TileObject;
        Ore ore = targetOre as Ore;
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        
        int amount = p_node.currentStateDuration * m_amountProducedPerTick;
        if (ore.count - amount < 0) {
            amount = ore.count;
        }
        
        if (targetOre.gridTileLocation != null) {
            if (ore.count <= 0) {
                targetOre.gridTileLocation.structure.RemovePOI(targetOre);
            }
        }

        if (amount <= 0) {
            return null;
        }
        ore.count = (int)Mathf.Clamp(ore.count - amount, 0f, 1000f);
        MetalPile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(ore.providedMetal.ConvertResourcesToTileObjectType());
        p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(amount * _coinGainMultiplier));
        matsToHaul.SetResourceInPile(amount);
        tileToSpawnPile.structure.AddPOI(matsToHaul, tileToSpawnPile);
        ProduceLogs(p_node);
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(12, p_node.actor);
        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        Ore targetOre = p_node.target as Ore;
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString() + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(targetOre.providedMetal.ConvertResourcesToTileObjectType().ToString());
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log, true);
    }
}