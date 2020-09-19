using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class Inspired : Interrupt {
        public Inspired() : base(INTERRUPT.Inspired) {
            duration = 0;
            isSimulateneous = true;
            shouldShowNotif = false;
            interruptIconString = GoapActionStateDB.Happy_Icon;
            logTags = new[] {LOG_TAG.Social, LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.needsComponent.AdjustHope(5f);
            return true;
        }
        #endregion
    }
}
