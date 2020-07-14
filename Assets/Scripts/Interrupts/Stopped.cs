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
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog,
            ActualGoapNode node = null) {
            bool executed = base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, node);
            Character targetCharacter = target as Character;
            if (node != null) {
                node.action.OnStoppedInterrupt(node);
                node.associatedJob?.CancelJob(false);
                executed = true;
            }
            targetCharacter.currentJob?.CancelJob(false);
            targetCharacter.currentJob?.StopJobNotDrop();
            if(actor != targetCharacter && node != null) {
                overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect_with_action");
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
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