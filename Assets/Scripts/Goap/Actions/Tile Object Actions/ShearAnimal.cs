
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UtilityScripts;

public class ShearAnimal : GoapAction {

    public int m_amountProducedPerTick = 1;
    private const float _coinGainMultiplier = 1.375f;
    public ShearAnimal() : base(INTERACTION_TYPE.SHEAR_ANIMAL) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
        shouldAddLogs = false;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Shear Animal Success", goapNode);
    }
    
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.target, node.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);

        return satisfied;
    }
    #endregion

    #region State Effects
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.currentStateDuration > 0) {
            ProduceMatsPile(node);
        }
    }
    public void AfterShearAnimalSuccess(ActualGoapNode p_node) {
        ResourcePile pile = ProduceMatsPile(p_node);
        if (pile != null && pile.resourceInPile > 0) {
            p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(pile);
        }
    }
    #endregion

    ResourcePile ProduceMatsPile(ActualGoapNode p_node) {
        Summon targetAnimal = p_node.target as Summon;
        ShearableAnimal animal = targetAnimal as ShearableAnimal;
        LocationGridTile tileToSpawnPile = p_node.actor.gridTileLocation;
        if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
            tileToSpawnPile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        
        int amount = p_node.currentStateDuration * m_amountProducedPerTick;

        if (animal.count - amount < 0) {
            amount = animal.count;
        }
        
        if (targetAnimal.gridTileLocation != null) {
            if (animal.count <= 0) {
                animal.isAvailableForShearing = false;
            }
        }
        if (amount <= 0) {
            return null;
        }
        animal.count = (int)Mathf.Clamp(animal.count - amount, 0f, 1000f);
        ResourcePile matsToHaul = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(animal.produceableMaterial);
        p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(amount * _coinGainMultiplier));
        matsToHaul.SetResourceInPile(amount);
        tileToSpawnPile.structure.AddPOI(matsToHaul, tileToSpawnPile);
        ProduceLogs(p_node);
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(12, p_node.actor);
        return matsToHaul;
    }

    public void ProduceLogs(ActualGoapNode p_node) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString() + " " + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters((p_node.target as Animal).produceableMaterial.ToString());
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log, true);
    }
}