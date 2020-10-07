using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class RevertFromBat : Interrupt {
        public RevertFromBat() : base(INTERRUPT.Revert_From_Bat) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            if (actor.behaviourComponent.isInVampireBatForm) {
                actor.behaviourComponent.SetIsInVampireBatForm(false);
                actor.movementComponent.AdjustSpeedModifier(-0.20f);
                if (actor.visuals != null) {
                    actor.visuals.UpdateAllVisuals(actor);
                }
            }
            return true;
        }
        #endregion
    }
}
