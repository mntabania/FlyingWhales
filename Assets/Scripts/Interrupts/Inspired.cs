using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Inspired : Interrupt {
        public Inspired() : base(INTERRUPT.Inspired) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Happy_Icon;
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
