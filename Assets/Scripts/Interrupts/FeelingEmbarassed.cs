using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingEmbarassed : Interrupt {
        public FeelingEmbarassed() : base(INTERRUPT.Feeling_Embarassed) {
            duration = 0;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.combatComponent.Flight(target, "embarassed");
            return true;
        }
        #endregion
    }
}