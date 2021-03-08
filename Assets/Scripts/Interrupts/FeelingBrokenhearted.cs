using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingBrokenhearted : Interrupt {
        public FeelingBrokenhearted() : base(INTERRUPT.Feeling_Brokenhearted) {
            duration = 5;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Heartbroken_Icon;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Needs, LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
            //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
            return true;
        }
        #endregion
    }
}
