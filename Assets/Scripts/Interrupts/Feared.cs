using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Feared : Interrupt {
        public Feared() : base(INTERRUPT.Feared) {
            duration = 0;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Cowering_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.combatComponent.Flight(interruptHolder.target, "Feared");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        #endregion
    }
}