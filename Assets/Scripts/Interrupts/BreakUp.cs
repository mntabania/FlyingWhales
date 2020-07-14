using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BreakUp : Interrupt {
        public BreakUp() : base(INTERRUPT.Break_Up) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Heartbroken_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.nonActionEventsComponent.NormalBreakUp(interruptHolder.target as Character, interruptHolder.identifier);
            return true;
        }
        #endregion
    }
}