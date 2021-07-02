
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;

public class MineStone : GoapAction {

    public int m_amountProducedPerTick = 1;
    private const float _coinGainMultiplier = 0.206f;

    public MineStone() : base(INTERACTION_TYPE.MINE_STONE) {
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
        SetState("Mine Success", goapNode);
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
    public void AfterMineSuccess(ActualGoapNode p_node) {
        ResourcePile pile = ProduceMatsPile(p_node);
        if (pile != null && pile.resourceInPile > 0) {
            p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(pile);
        }
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        TileObject targetStone = p_node.target as TileObject;
        Rock rock = targetStone as Rock;
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        
        int amount = p_node.currentStateDuration * m_amountProducedPerTick;
        if (rock.count - amount < 0) {
            amount = rock.count;
        }
        
        if (targetStone.gridTileLocation != null) {
            if (rock.count <= 0) {
                targetStone.gridTileLocation.structure.RemovePOI(targetStone);
            }
        }

        if (amount <= 0) {
            return null;
        }
        rock.count = (int)Mathf.Clamp(rock.count - amount, 0f, 1000f);
        StonePile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(amount * _coinGainMultiplier));
        matsToHaul.SetResourceInPile(amount);
        tileToSpawnPile.structure.AddPOI(matsToHaul, tileToSpawnPile);
        ProduceLogs(p_node);
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(8, p_node.actor);
        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString();
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log, true);
    }
}