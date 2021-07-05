using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;

namespace Interrupts {
    public class MentalBreak : Interrupt {
        public MentalBreak() : base(INTERRUPT.Mental_Break) {
            duration = 0;
            isSimulateneous = true;
            shouldShowNotif = true;
            interruptIconString = GoapActionStateDB.Anger_Icon;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Needs, LOG_TAG.Major};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, interruptHolder.actor.moodComponent.mentalBreakName, LOG_IDENTIFIER.STRING_1);
            //Removed this because overridden logs are already added after in AddEffectLog
            //interruptHolder.actor.logComponent.RegisterLog(overrideEffectLog, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}
