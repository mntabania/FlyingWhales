
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class FindFish : GoapAction {

    public int m_amountProducedPerTick = 10;
    public FishPile m_matsToHaul;
    public int m_count = 0;

    public FindFish() : base(INTERACTION_TYPE.FIND_FISH) {
        actionIconString = GoapActionStateDB.Fish_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        m_matsToHaul = null;
        m_count = 0;
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
            if (m_matsToHaul != null) {
                p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(m_matsToHaul);
            }
        }
        if (m_count <= 0) {
            ProduceNoneLogs(p_node);
        } else {
            ProduceLogsPerTick(p_node);
        }
    }
    #endregion

    #region State Effects
    public void AfterFindFishSuccess(ActualGoapNode p_node) {
        if (m_matsToHaul != null) {
            p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(m_matsToHaul);
        }
        if (m_count <= 0) {
            ProduceNoneLogs(p_node);
        }
    }

    public void PerTickFindFishSuccess(ActualGoapNode p_node) {
        m_matsToHaul = null;
        m_count = 0;
        int pileCount = p_node.actor.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.FISH_PILE);
        if (UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(0, 100) < 85) {
            if (pileCount > 0 || p_node.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject() != null) {
                m_count += 10;
                ProduceMatsPile(p_node);
                ProduceLogsPerTick(p_node);
            } else { 
                
            }
            
        }
        //p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(ProduceMatsPile(p_node));
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        if (m_matsToHaul == null) {
            int pileCount = p_node.actor.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.FISH_PILE);
            if (pileCount > 0) {
                for (int x = 0; x < p_node.actor.gridTileLocation.neighbourList.Count; ++x) {
                    if (p_node.actor.gridTileLocation.neighbourList[x].tileObjectComponent.objHere?.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE) {
                        m_matsToHaul = p_node.actor.gridTileLocation.neighbourList[x].tileObjectComponent.objHere as FishPile;
                        break;
                    }
                }
                m_matsToHaul.AdjustResourceInPile(m_count);
            } else {
                LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
                if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
                    tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
                }
                m_matsToHaul = InnerMapManager.Instance.CreateNewTileObject<FishPile>(TILE_OBJECT_TYPE.FISH_PILE);
                tileToSpawnPile.structure.AddPOI(m_matsToHaul, tileToSpawnPile);
                p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(20, p_node.actor);
                m_matsToHaul.SetResourceInPile(m_count);
            }
        }
        return m_matsToHaul;
    }

    public void ProduceNoneLogs(ActualGoapNode p_node) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "none_produced", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        p_node.LogAction(log);
    }

    public void ProduceLogsPerTick(ActualGoapNode p_node) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources_per_tick", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        p_node.LogAction(log);
    }
}