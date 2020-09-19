using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class FeelingConcerned : Interrupt {
        public FeelingConcerned() : base(INTERRUPT.Feeling_Concerned) {
            duration = 0;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            shouldAddLogs = false;
            logTags = new[] {LOG_TAG.Social, LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            return interruptHolder.actor.jobComponent.CreateGoToJob(interruptHolder.target);
            //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, target, actor);
            //return actor.jobQueue.AddJobInQueue(job);
        }
        #endregion
    }
}