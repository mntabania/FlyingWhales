using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Daydream : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Daydream() : base(INTERACTION_TYPE.DAYDREAM) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        actionIconString = GoapActionStateDB.Daydream_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Daydream Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        int cost = UtilityScripts.Utilities.Rng.Next(90, 131);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Times Daydreamed > 5)";
#endif
        }
        int timesCost = 10 * numOfTimesActionDone;
        cost += timesCost;
#if DEBUG_LOG
        costLog += $" +{timesCost.ToString()}(10 x Times Daydreamed)";
#endif
        if (actor.traitContainer.HasTrait("Lazy")) {
            cost -= 25;
#if DEBUG_LOG
            costLog += $" -25(Actor is Lazy)";
#endif
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

#region Effects
    public void PreDaydreamSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(2f);
        goapNode.actor.needsComponent.AdjustStamina(0.33f);
    }
    //public void AfterDaydreamSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            //if (actor.traitContainer.HasTrait("Disillusioned")) {
            //    return false;
            //}
            return actor == poiTarget;
        }
        return false;
    }
#endregion
}