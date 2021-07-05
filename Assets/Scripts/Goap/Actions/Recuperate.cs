using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;
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

    //public override void OnActionStarted(ActualGoapNode p_node) {
    //    base.OnActionStarted(p_node);
    //}

    public override void OnStopWhilePerforming(ActualGoapNode p_node) {
        base.OnStopWhilePerforming(p_node);
        p_node.actor.traitContainer.RemoveTrait(p_node.actor, "Recuperating");
        //p_node.actor.traitContainer.RemoveTrait(p_node.actor, "Resting");
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        //node.actor.traitContainer.RemoveTrait(node.actor, "Resting");
        node.actor.traitContainer.RemoveTrait(node.actor, "Recuperating");
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
    public void PreRecuperateSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Recuperating");
    }
    public void AfterRecuperateSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Recuperating");
        goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Poisoned");
        goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Plagued");
        goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Injured");

        LocationStructure targetStructure = goapNode.poiTarget.gridTileLocation?.structure;
        if (targetStructure != null && targetStructure.structureType == STRUCTURE_TYPE.HOSPICE) {
            if (targetStructure is ManMadeStructure mmStructure) {
                if (mmStructure.HasAssignedWorker()) {
                    //only added coins to first worker since we expect that the hospice only has 1 worker.
                    //if that changes, this needs to be changed as well.
                    string assignedWorkerID = mmStructure.assignedWorkerIDs[0];
                    Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                    assignedWorker.moneyComponent.AdjustCoins(28);
                }
                // Character assignedWorker = mmStructure.assignedWorker;
                // if (assignedWorker != null) {
                //     assignedWorker.moneyComponent.AdjustCoins(10);
                // }
            }
        }
    }

    public void PerTickRecuperateSuccess(ActualGoapNode p_node) {
        Character actor = p_node.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;
        // if (needsComponent.currentSleepTicks == 1) { //If sleep ticks is down to 1 tick left, set current duration to end duration so that the action will end now, we need this because the character must only sleep the remaining hours of his sleep if ever that character is interrupted while sleeping
        //     goapNode.OverrideCurrentStateDuration(goapNode.currentState.duration);
        // }
        needsComponent.AdjustTiredness(0.417f);
        if (!IsSubjectForRecuperate(p_node)) {
            p_node.associatedJob.CancelJob();
            return;
        }
        
    }
	#endregion
}