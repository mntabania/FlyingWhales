using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Cowering : Interrupt {
        public Cowering() : base(INTERRUPT.Cowering) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Cowering_Icon;
            logTags = new[] {LOG_TAG.Combat};
        }

        //#region Overrides
        //public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
        //    actor.CancelAllJobs();
        //    return true;
        //}
        //#endregion
        
        #region Overrides
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            Log effectLog = base.CreateEffectLog(actor, target);
            if (effectLog != null && actor.interruptComponent.currentInterrupt != null) {
                effectLog.AddToFillers(null, actor.interruptComponent.currentInterrupt.reason, LOG_IDENTIFIER.STRING_1);
                return effectLog;
            }
            return default;
        }
        public override void AddAdditionalFillersToThoughtLog(Log log, Character actor) {
            base.AddAdditionalFillersToThoughtLog(log, actor);
            if (log != null && actor.interruptComponent.currentInterrupt != null) {
                log.AddToFillers(null, actor.interruptComponent.currentInterrupt.reason, LOG_IDENTIFIER.STRING_1);
            }
        }
        #endregion
    }
}