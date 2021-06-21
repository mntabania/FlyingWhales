using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;

namespace Interrupts {
    public class Stopped : Interrupt {
        public Stopped() : base(INTERRUPT.Stopped) {
            duration = 0;
            //doesStopCurrentAction = true;
            //doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isSimulateneous = true;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog,
            ActualGoapNode node = null) {
            bool executed = base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, node);
            Character targetCharacter = interruptHolder.target as Character;
            GoapAction p_action = node.action;
            if (node != null) {
                p_action.OnStoppedInterrupt(node);
                if (node.associatedJob != null && !node.associatedJob.hasBeenReset && node.associatedJob.originalOwner != null) {
                    node.associatedJob.CancelJob();
                }
                executed = true;
            }
            targetCharacter.currentJob?.CancelJob();
            targetCharacter.currentJob?.StopJobNotDrop();
            if (interruptHolder.actor != targetCharacter && p_action != null) {
                //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect_with_action", null, logTags);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                overrideEffectLog.AddToFillers(null, p_action.name, LOG_IDENTIFIER.STRING_1);
            }
            return executed;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (actor == target) {
                return default; //do not create log if actor was stopped by itself.
            }
            return base.CreateEffectLog(actor, target);
        }
        #endregion
    }
}