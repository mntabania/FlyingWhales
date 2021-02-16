using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class BreakUp : Interrupt {
        public BreakUp() : base(INTERRUPT.Break_Up) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Heartbroken_Icon;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.nonActionEventsComponent.NormalBreakUp(interruptHolder.target as Character, interruptHolder.identifier);
            return true;
        }
        #endregion
    }
}