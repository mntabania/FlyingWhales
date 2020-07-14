using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class FeelingLazy : Interrupt {
        public FeelingLazy() : base(INTERRUPT.Feeling_Lazy) {
            duration = 0;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if (!actor.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, actor, actor);
                actor.jobQueue.AddJobInQueue(job);
                //bool triggerBrokenhearted = false;
                //Heartbroken heartbroken = actor.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                //if (heartbroken != null) {
                //    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                //}
                //if (!triggerBrokenhearted) {
                //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, owner, owner);
                //    owner.jobQueue.AddJobInQueue(job);
                //    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "trigger_lazy");
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