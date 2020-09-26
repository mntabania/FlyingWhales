﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class LeaveParty : Interrupt {
        public LeaveParty() : base(INTERRUPT.Leave_Party) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Party party = interruptHolder.actor.partyComponent.currentParty;
            if (party != null) {
                party.RemoveMember(interruptHolder.actor);

                overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect", providedTags: LOG_TAG.Party);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(party, party.partyName, LOG_IDENTIFIER.PARTY_1);
                overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_2);
            }
            return true;
        }
        #endregion
    }
}
