using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class MentalBreak : Interrupt {
        public MentalBreak() : base(INTERRUPT.Mental_Break) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Anger_Icon;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Needs};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, interruptHolder.actor.moodComponent.mentalBreakName, LOG_IDENTIFIER.STRING_1);
            interruptHolder.actor.logComponent.RegisterLog(overrideEffectLog, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}
