using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class Chat : Interrupt {
        public Chat() : base(INTERRUPT.Chat) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Social_Icon;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.nonActionEventsComponent.ForceChatCharacter(interruptHolder.target as Character, overrideEffectLog);
            return true;
        }
        #endregion
    }
}