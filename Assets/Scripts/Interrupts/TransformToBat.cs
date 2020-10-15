using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class TransformToBat : Interrupt {
        public TransformToBat() : base(INTERRUPT.Transform_To_Bat) {
            duration = 1;
            //isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            if (!actor.behaviourComponent.isInVampireBatForm) {
                actor.behaviourComponent.SetIsInVampireBatForm(true);
                actor.movementComponent.AdjustSpeedModifier(0.20f);
                actor.movementComponent.SetTagAsTraversable(InnerMapManager.Obstacle_Tag);
                if (actor.visuals != null) {
                    actor.visuals.UpdateAllVisuals(actor);
                }
            }
            return true;
        }
        #endregion
    }
}
