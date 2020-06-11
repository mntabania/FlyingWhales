using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stopped : Interrupt {
        public Stopped() : base(INTERRUPT.Stopped) {
            duration = 0;
            //doesStopCurrentAction = true;
            //doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog, ActualGoapNode node = null) {
            bool executed = base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog, node);
            if (node != null) {
                node.action.OnStoppedInterrupt(node);
                node.associatedJob?.CancelJob(false);
                executed = true;
            }
            actor.currentJob?.CancelJob(false);
            actor.currentJob?.StopJobNotDrop();
            if(actor != target && node != null) {
                overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect_with_action");
                overrideEffectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                overrideEffectLog.AddToFillers(null, node.action.name, LOG_IDENTIFIER.STRING_1);
            }
            return executed;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (actor == target) {
                return null; //do not create log if actor was stopped by itself.
            }
            return base.CreateEffectLog(actor, target);
        }
        #endregion
    }
}