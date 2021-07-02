
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;

public class HarvestCrops : GoapAction {

    public int m_amountProducedPerTick = 1;
    private const float _coinGainMultiplier = 0.344f;
    public HarvestCrops() : base(INTERACTION_TYPE.HARVEST_CROPS) {
        actionIconString = GoapActionStateDB.Harvest_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
        shouldAddLogs = false;
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
        if (node.currentStateDuration > 0) {
            ProduceMatsPile(node);
        }
    }
    #endregion

    #region State Effects
    public void AfterHarvestCropsSuccess(ActualGoapNode p_node) {
        ResourcePile pile = ProduceMatsPile(p_node);
        if (pile != null && pile.resourceInPile > 0) {
            p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(ProduceMatsPile(p_node));
        }
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        TileObject targetCrop = p_node.target as TileObject;
        Crops crop = targetCrop as Crops;
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        
        int amount = p_node.currentStateDuration * m_amountProducedPerTick;
        if (crop.count - amount < 0) {
            amount = crop.count;
        }
        
        if (targetCrop.gridTileLocation != null) {
            if (crop.count <= 0) {
                targetCrop.gridTileLocation.structure.RemovePOI(targetCrop);
            }
        }

        if (amount <= 0) {
            return null;
        }
        crop.count = (int)Mathf.Clamp(crop.count - amount, 0f, 1000f);
        FoodPile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(crop.producedObjectOnHarvest);
        p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(amount * _coinGainMultiplier));
        matsToHaul.SetResourceInPile(amount);
        tileToSpawnPile.structure.AddPOI(matsToHaul, tileToSpawnPile);
        ProduceLogs(p_node, crop);
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(8, p_node.actor);
        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node, Crops pcrops) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString() + " " + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(pcrops.producedObjectOnHarvest.ToString());
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log, true);
    }
}