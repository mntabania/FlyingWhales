using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Cowering : Interrupt {
        public Cowering() : base(INTERRUPT.Cowering) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Cowering_Icon;
        }

        //#region Overrides
        //public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
        //    actor.CancelAllJobs();
        //    return true;
        //}
        //#endregion
    }
}