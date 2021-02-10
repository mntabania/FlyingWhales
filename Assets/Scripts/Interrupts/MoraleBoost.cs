using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class MoraleBoost : Interrupt {
        public MoraleBoost() : base(INTERRUPT.Morale_Boost) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new LOG_TAG[] { LOG_TAG.Party, LOG_TAG.Needs };
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            actor.needsComponent.ResetFullnessMeter();
            actor.needsComponent.ResetHappinessMeter();
            actor.needsComponent.ResetTirednessMeter();
            return true;
        }
        #endregion
    }
}
