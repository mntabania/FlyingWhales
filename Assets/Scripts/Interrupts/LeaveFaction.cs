using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LeaveFaction : Interrupt {
        public LeaveFaction() : base(INTERRUPT.Leave_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Faction prevFaction = interruptHolder.actor.faction;
            if (interruptHolder.actor.ChangeFactionTo(FactionManager.Instance.vagrantFaction)) {
                overrideEffectLog  = new Log(GameManager.Instance.Today(), "Interrupt", "Leave Faction", interruptHolder.identifier);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                return true;
            }
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        #endregion
    }
}