using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingEmbarassed : Interrupt {
        public FeelingEmbarassed() : base(INTERRUPT.Feeling_Embarassed) {
            duration = 0;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Shock_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.combatComponent.Flight(interruptHolder.target, "embarassed");
            return true;
        }
        #endregion
    }
}