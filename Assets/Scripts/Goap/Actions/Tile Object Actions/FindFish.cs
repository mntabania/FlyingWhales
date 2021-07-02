
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class FindFish : GoapAction {

    public int m_amountProducedPerTick = 16;
    private const float _coinGainMultiplier = 0.516f;

    public FindFish() : base(INTERACTION_TYPE.FIND_FISH) {
        actionIconString = GoapActionStateDB.Fish_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
        shouldAddLogs = false;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        goapNode.actor.jobComponent.fishPile = null;
        goapNode.actor.jobComponent.producedFish = 0;
        SetState("Find Fish Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        return goapActionInvalidity;
    }
    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        //Action is invalid if the target is unavailable and the action cannot be advertised if target is unavailable
        if ((poiTarget.IsAvailable() == false && !canBeAdvertisedEvenIfTargetIsUnavailable) || poiTarget.gridTileLocation == null) {
            return true;
        }
        if (actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE && actor.currentRegion != poiTarget.gridTileLocation.structure.region) {
            return true;
        }
        LocationGridTile targetTile = poiTarget.gridTileLocation;
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_OTHER_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile, true) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEARBY || actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION
            || actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B || actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE) {
            //if the action type is NEARBY, RANDOM_LOCATION, RANDOM_LOCATION_B, OVERRIDE, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile, true) == false) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);

        return satisfied;
    }
    public override void OnStopWhilePerforming(ActualGoapNode p_node) {
        base.OnStopWhilePerforming(p_node);
        if (p_node.currentStateDuration > 0) {
            if (p_node.actor.jobComponent.fishPile != null) {
                p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(p_node.actor.jobComponent.fishPile);
            }
        }
        if (p_node.actor.jobComponent.producedFish <= 0) {
            ProduceNoneLogs(p_node);
        } else {
            ProduceLogsPerTick(p_node);
        }
        p_node.actor.jobComponent.producedFish = 0;
        p_node.actor.jobComponent.fishPile = null;
    }
    #endregion

    #region State Effects
    public void AfterFindFishSuccess(ActualGoapNode p_node) {
        p_node.actor.jobQueue.CancelAllJobs(JOB_TYPE.STOCKPILE_FOOD);
        if (p_node.actor.jobComponent.fishPile != null) {
            p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(p_node.actor.jobComponent.fishPile);
        }
        if (p_node.actor.jobComponent.producedFish <= 0) {
            ProduceNoneLogs(p_node);
        }
        p_node.actor.jobComponent.producedFish = 0;
        p_node.actor.jobComponent.fishPile = null;
    }

    public void PerTickFindFishSuccess(ActualGoapNode p_node) {
        p_node.actor.jobComponent.fishPile = null;
        int pileCount = p_node.actor.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.FISH_PILE);
        if (ChanceData.RollChance(CHANCE_TYPE.Find_Fish)) {
            if (pileCount > 0 || p_node.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject() != null) {
                p_node.actor.jobComponent.producedFish += m_amountProducedPerTick;
                ProduceMatsPile(p_node);
                ProduceLogsPerTick(p_node);
            }
        }
        //p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(ProduceMatsPile(p_node));
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode goapNode) {
        if (goapNode.actor.jobComponent.fishPile == null) {
            int pileCount = goapNode.actor.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.FISH_PILE);
            if (pileCount > 0) {
                for (int x = 0; x < goapNode.actor.gridTileLocation.neighbourList.Count; ++x) {
                    if (goapNode.actor.gridTileLocation.neighbourList[x].tileObjectComponent.objHere?.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE) {
                        goapNode.actor.jobComponent.fishPile = goapNode.actor.gridTileLocation.neighbourList[x].tileObjectComponent.objHere as FishPile;
                        break;
                    }
                }
                goapNode.actor.jobComponent.fishPile.AdjustResourceInPile(m_amountProducedPerTick);
                goapNode.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(m_amountProducedPerTick * _coinGainMultiplier));
            } else {
                LocationGridTile tileToSpawnPile = goapNode.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
                if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
                    tileToSpawnPile = goapNode.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
                }
                goapNode.actor.jobComponent.fishPile = InnerMapManager.Instance.CreateNewTileObject<FishPile>(TILE_OBJECT_TYPE.FISH_PILE);
                tileToSpawnPile.structure.AddPOI(goapNode.actor.jobComponent.fishPile, tileToSpawnPile);
                goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(4, goapNode.actor);
                goapNode.actor.jobComponent.fishPile.SetResourceInPile(m_amountProducedPerTick);
                goapNode.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(m_amountProducedPerTick * _coinGainMultiplier));
            }
        }
        return goapNode.actor.jobComponent.fishPile;
    }

    public void ProduceNoneLogs(ActualGoapNode p_node) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "none_produced", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        p_node.LogAction(log, true);
    }

    public void ProduceLogsPerTick(ActualGoapNode p_node) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources_per_tick", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        p_node.LogAction(log, true);
    }
}