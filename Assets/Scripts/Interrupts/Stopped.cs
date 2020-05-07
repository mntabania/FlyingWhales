using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stopped : Interrupt {
        public Stopped() : base(INTERRUPT.Stopped) {
            duration = 0;
            //doesStopCurrentAction = true;
            //doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog, ActualGoapNode node = null) {
            bool executed = base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog, node);
            if (node != null) {
                node.action.OnStoppedInterrupt(node);
                node.associatedJob?.CancelJob(false);
                executed = true;
            }
            actor.currentJob?.CancelJob(false);
            actor.currentJob?.StopJobNotDrop();
            return executed;
        }
        #endregion
    }
}