using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class AbominationDeath : Interrupt {
        public AbominationDeath() : base(INTERRUPT.Abomination_Death) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.Death("Abomination Germ", interrupt: this);
            return true;
        }
        #endregion
    }
}