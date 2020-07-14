using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Grieving : Interrupt {
        public Grieving() : base(INTERRUPT.Grieving) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
            interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, interruptHolder.target, "grieving");
            return true;
        }
        #endregion
    }
}