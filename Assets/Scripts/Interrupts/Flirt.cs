using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Flirt : Interrupt {
        public Flirt() : base(INTERRUPT.Flirt) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.nonActionEventsComponent.NormalFlirtCharacter(target as Character, ref overrideEffectLog);
            return true;
        }
        #endregion
    }
}