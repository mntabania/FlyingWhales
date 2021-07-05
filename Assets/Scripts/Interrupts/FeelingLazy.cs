using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Traits;
using Inner_Maps.Location_Structures;

namespace Interrupts {
    public class FeelingLazy : Interrupt {
        public FeelingLazy() : base(INTERRUPT.Feeling_Lazy) {
            duration = 0;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            logTags = new[] {LOG_TAG.Needs};
            shouldShowNotif = false;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            if (!actor.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, interruptHolder.actor, interruptHolder.actor);
                UtilityScripts.JobUtilities.PopulatePriorityLocationsForHappinessRecovery(actor, job);
                job.SetDoNotRecalculate(true);
                interruptHolder.actor.jobQueue.AddJobInQueue(job);
                //bool triggerBrokenhearted = false;
                //Heartbroken heartbroken = actor.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                //if (heartbroken != null) {
                //    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                //}
                //if (!triggerBrokenhearted) {
                //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, owner, owner);
                //    owner.jobQueue.AddJobInQueue(job);
                //    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "trigger_lazy");
                //    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //    owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                //} else {
                //    heartbroken.TriggerBrokenhearted();
                //}
                return true;
            }
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        #endregion
    }
}