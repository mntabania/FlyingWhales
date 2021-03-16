using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class JoinParty : Interrupt {
        public JoinParty() : base(INTERRUPT.Join_Party) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Party};
        }


        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(interruptHolder.target is Character targetCharacter) {
                Party party = targetCharacter.partyComponent.currentParty;
                if (party != null) {
                    party.AddMember(interruptHolder.actor);
                }
            }
            return true;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            Log effectLog = base.CreateEffectLog(actor, target);
            if (effectLog != null && actor.partyComponent.hasParty) {
                effectLog.AddToFillers(actor.partyComponent.currentParty, actor.partyComponent.currentParty.partyName, LOG_IDENTIFIER.PARTY_1);
                return effectLog;
            }
            return default;
        }
        #endregion
    }
}
