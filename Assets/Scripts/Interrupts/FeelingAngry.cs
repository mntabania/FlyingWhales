using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingAngry : Interrupt {
        public FeelingAngry() : base(INTERRUPT.Feeling_Angry) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Anger_Icon;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Combat};
        }

        //#region Overrides
        //public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
        //    interruptHolder.actor.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
        //    Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
        //    return true;
        //}
        //#endregion
    }
}
