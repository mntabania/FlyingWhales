using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Worried : Interrupt {
        public Worried() : base(INTERRUPT.Worried) {
            duration = 3;
            doesStopCurrentAction = true;
            //isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            //isIntel = true;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Worried", interruptHolder.target as Character);
            //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
            //overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "cry");
            //overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //overrideEffectLog.AddToFillers(null, actor.interruptComponent.identifier, LOG_IDENTIFIER.STRING_1);
            return true;
        }
        #endregion
    }
}
