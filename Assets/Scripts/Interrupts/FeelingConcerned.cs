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
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if (interruptHolder.actor.marker != null && !interruptHolder.actor.marker.inVisionPOIs.Contains(interruptHolder.target)) {
                return interruptHolder.actor.jobComponent.CreateGoToJob(interruptHolder.target);    
            }
            return false;
        }
        #endregion
    }
}