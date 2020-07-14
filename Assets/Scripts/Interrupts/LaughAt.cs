using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LaughAt : Interrupt {
        public LaughAt() : base(INTERRUPT.Laugh_At) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Mock_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character targetCharacter = interruptHolder.target as Character;
            if (targetCharacter.canWitness) {
                targetCharacter.traitContainer.AddTrait(targetCharacter, "Ashamed");
                return true;
            }
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        #endregion
    }
}
