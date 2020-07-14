using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Chat : Interrupt {
        public Chat() : base(INTERRUPT.Chat) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Social_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            actor.nonActionEventsComponent.ForceChatCharacter(target as Character, ref overrideEffectLog);
            return true;
        }
        #endregion
    }
}