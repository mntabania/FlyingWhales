
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class Recuperate : GoapAction {

    public Recuperate() : base(INTERACTION_TYPE.RECUPERATE) {
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Recuperate Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }

    public override void OnActionStarted(ActualGoapNode p_node) {
        base.OnActionStarted(p_node);
    }

    public override void OnStopWhilePerforming(ActualGoapNode p_node) {
        base.OnStopWhilePerforming(p_node);
    }

    #endregion

    bool IsSubjectForRecuperate(ActualGoapNode p_node) {
        return (p_node.actor.traitContainer.HasTrait("Injured") || p_node.actor.traitContainer.HasTrait("Plagued") || p_node.actor.traitContainer.HasTrait("Poisoned"));
    }

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);

        return satisfied;
    }
    #endregion

    #region State Effects
    public void AfterRecuperateSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Poisoned", goapNode.actor);
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Plagued", goapNode.actor);
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Injured", goapNode.actor);
    }

    public void PerTickRecuperateSuccess(ActualGoapNode p_node) {
        if (!IsSubjectForRecuperate(p_node)) {
            p_node.associatedJob.CancelJob();
        }
    }
    #endregion
}