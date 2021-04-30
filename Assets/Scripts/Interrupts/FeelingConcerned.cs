using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class FeelingConcerned : Interrupt {
        public FeelingConcerned() : base(INTERRUPT.Feeling_Concerned) {
            interruptIconString = GoapActionStateDB.Sad_Icon;
            duration = 0;
            //doesStopCurrentAction = true;
            isSimulateneous = true;
            shouldAddLogs = false;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        //Note: Removed this temporarily because if the target is being carried by another character when the actor becomes feeling concerned,
        //the actor will not do anything because he will wait until the target is dropped before doing the job
        //This is because we do not let another character do an action to another character if it is being carried by another character to avoid conflicts in action
        //see PerformJob in Character script in line 4095

        //public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
        //    if (interruptHolder.actor.hasMarker && !interruptHolder.actor.marker.inVisionPOIs.Contains(interruptHolder.target)) {
        //        return interruptHolder.actor.jobComponent.CreateGoToJob(interruptHolder.target);    
        //    }
        //    return false;
        //}
        #endregion
    }
}