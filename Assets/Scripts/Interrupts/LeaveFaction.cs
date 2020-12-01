using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class LeaveFaction : Interrupt {
        public LeaveFaction() : base(INTERRUPT.Leave_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Faction prevFaction = interruptHolder.actor.faction;
            Faction targetFaction;
            if (interruptHolder.actor is Summon summon) {
                targetFaction = summon.defaultFaction;
            } else {
                targetFaction = FactionManager.Instance.vagrantFaction;
            }
            
            if (interruptHolder.actor.ChangeFactionTo(targetFaction)) {
                overrideEffectLog  = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Leave Faction", interruptHolder.identifier, null, logTags);
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