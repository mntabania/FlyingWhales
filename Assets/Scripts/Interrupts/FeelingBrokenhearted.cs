using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingBrokenhearted : Interrupt {
        public FeelingBrokenhearted() : base(INTERRUPT.Feeling_Brokenhearted) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Heartbroken_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
            Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
            return true;
        }
        #endregion
    }
}
