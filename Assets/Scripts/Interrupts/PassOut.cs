﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class PassOut : Interrupt {
        public PassOut() : base(INTERRUPT.Pass_Out) {
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_1);
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
            return true;
        }
        #endregion
    }
}