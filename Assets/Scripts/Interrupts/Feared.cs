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
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            actor.combatComponent.Flight(target, "Feared");
            return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog, goapNode);
        }
        #endregion
    }
}