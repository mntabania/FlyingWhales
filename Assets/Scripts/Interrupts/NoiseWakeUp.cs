using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class NoiseWakeUp : Interrupt {
        public NoiseWakeUp() : base(INTERRUPT.Noise_Wake_Up) {
            duration = 1;
            doesDropCurrentJob = true;
            doesStopCurrentAction = true;
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            logTags = new LOG_TAG[] { LOG_TAG.Social };
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            // actor.needsComponent.ResetSleepTicks();
            // actor.needsComponent.SetHasCancelledSleepSchedule(false);
            return true;
        }
        #endregion
    }
}
