
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;

public class GatherHerb : GoapAction {

    public int m_amountProducedPerTick = 1;

    public GatherHerb() : base(INTERACTION_TYPE.GATHER_HERB) {
        actionIconString = GoapActionStateDB.Chop_Icon;
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
        SetState("Gather Herb Success", goapNode);
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
            ProduceHerbPlant(node);
        }
    }
    #endregion

    #region State Effects
    public void AfterGatherHerbSuccess(ActualGoapNode p_node) {
        p_node.actor.jobComponent.CreateDropItemJob(JOB_TYPE.GATHER_HERB, ProduceHerbPlant(p_node), (p_node.actor).structureComponent.workPlaceStructure);
    }
    #endregion

    HerbPlant ProduceHerbPlant(ActualGoapNode p_node) {
        HerbPlant herbPlant = p_node.target as HerbPlant;
        Assert.IsNotNull(herbPlant);
        p_node.actor.PickUpItem(herbPlant);

        return herbPlant;
    }
}