using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Angered : Interrupt {
        public Angered() : base(INTERRUPT.Angered) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Anger_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            actor.traitContainer.AddTrait(actor, "Angry",  target as Character);
            return true;
        }
        #endregion
    }
}
