using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class MentalBreak : Interrupt {
        public MentalBreak() : base(INTERRUPT.Mental_Break) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, actor.moodComponent.mentalBreakName, LOG_IDENTIFIER.STRING_1);
            actor.logComponent.RegisterLog(overrideEffectLog, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}
