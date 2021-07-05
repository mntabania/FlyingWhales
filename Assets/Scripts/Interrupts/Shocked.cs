using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Shocked : Interrupt {
        public const string Copycat_Reason = "Saw a copycat";
        public const string Witness_Reason = "Witnessed something surprising";
        
        public Shocked() : base(INTERRUPT.Shocked) {
            duration = 2;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            shouldShowNotif = false;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            Log effectLog = base.CreateEffectLog(actor, target);
            if (effectLog != null && actor.interruptComponent.currentInterrupt != null) {
                string reasonToUse = string.IsNullOrEmpty(actor.interruptComponent.currentInterrupt.reason) ? Witness_Reason : actor.interruptComponent.currentInterrupt.reason;
                effectLog.AddToFillers(null, reasonToUse, LOG_IDENTIFIER.STRING_1);
                return effectLog;
            }
            return default;
        }
        public override void AddAdditionalFillersToThoughtLog(Log log, Character actor) {
            base.AddAdditionalFillersToThoughtLog(log, actor);
            if (log != null && actor.interruptComponent.currentInterrupt != null) {
                string reasonToUse = string.IsNullOrEmpty(actor.interruptComponent.currentInterrupt.reason) ? Witness_Reason : actor.interruptComponent.currentInterrupt.reason;
                log.AddToFillers(null, reasonToUse, LOG_IDENTIFIER.STRING_1);
            }
        }
        #endregion
    }
}