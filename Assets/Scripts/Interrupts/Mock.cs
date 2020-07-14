using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Mock : Interrupt {
        public Mock() : base(INTERRUPT.Mock) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Mock_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character targetCharacter = target as Character;
            targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, actor, "Base", -3);
            return true;
        }
        #endregion
    }
}