using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Grieving : Interrupt {
        public Grieving() : base(INTERRUPT.Grieving) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, actor, "grieving");
            return true;
        }
        #endregion
    }
}