using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BreakUp : Interrupt {
        public BreakUp() : base(INTERRUPT.Break_Up) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.nonActionEventsComponent.NormalBreakUp(target as Character, actor.interruptComponent.identifier);
            return true;
        }
        #endregion
    }
}