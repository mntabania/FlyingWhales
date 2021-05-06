using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;

namespace Interrupts {
    public class JoinFaction : Interrupt {
        public JoinFaction() : base(INTERRUPT.Join_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(interruptHolder.target is Character targetCharacter) {
                Faction factionToJoinTo = targetCharacter.faction;
                bool bypassIdeology = interruptHolder.identifier == "join_faction_necro";
                if (interruptHolder.actor.ChangeFactionTo(factionToJoinTo, bypassIdeology)) {
                    //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                    overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Join Faction", interruptHolder.identifier, null, logTags);
                    overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    overrideEffectLog.AddToFillers(factionToJoinTo, factionToJoinTo.name, LOG_IDENTIFIER.FACTION_1);
                    overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    return true;
                } else {
                    //If character could not join faction, it must join the default faction because we do not want character to have null faction
                    interruptHolder.actor.ChangeToDefaultFaction();
                }
            }
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        #endregion
    }
}