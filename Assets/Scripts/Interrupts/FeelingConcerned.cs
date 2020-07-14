using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingConcerned : Interrupt {
        public FeelingConcerned() : base(INTERRUPT.Feeling_Concerned) {
            duration = 0;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            shouldAddLogs = false;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            return actor.jobComponent.CreateGoToJob(target);
            //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, target, actor);
            //return actor.jobQueue.AddJobInQueue(job);
        }
        #endregion
    }
}