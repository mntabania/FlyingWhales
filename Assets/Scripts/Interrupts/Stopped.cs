using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stopped : Interrupt {
        public Stopped() : base(INTERRUPT.Stopped) {
            duration = 0;
            //doesStopCurrentAction = true;
            //doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog, ActualGoapNode node = null) {
            if(node != null) {
                node.action.OnStoppedInterrupt(node);
                node.associatedJob?.CancelJob(false);
                return true;
            }
            return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog, node);
        }
        #endregion
    }
}