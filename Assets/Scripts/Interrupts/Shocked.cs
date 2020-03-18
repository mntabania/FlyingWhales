using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Shocked : Interrupt {
        public Shocked() : base(INTERRUPT.Shocked) {
            duration = 2;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Shock_Icon;
        }

        //#region Overrides
        //public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
        //    actor.traitContainer.AddTrait(actor, "Resting");
        //    return true;
        //}
        //public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
        //    actor.traitContainer.RemoveTrait(actor, "Resting");
        //    return true;
        //}
        //#endregion
    }
}