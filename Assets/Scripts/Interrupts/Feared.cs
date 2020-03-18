using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Feared : Interrupt {
        public Feared() : base(INTERRUPT.Feared) {
            duration = 0;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Hostile_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.combatComponent.Flight(target, "Feared");
            return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
        }
        #endregion
    }
}