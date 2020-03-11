using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class JoinFaction : Interrupt {
        public JoinFaction() : base(INTERRUPT.Join_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            if(target is Character) {
                Character targetCharacter = target as Character;
                Faction factionToJoinTo = targetCharacter.faction;
                if (actor.ChangeFactionTo(factionToJoinTo)) {
                    overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Join Faction", actor.interruptComponent.identifier);
                    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    overrideEffectLog.AddToFillers(factionToJoinTo, factionToJoinTo.name, LOG_IDENTIFIER.FACTION_1);
                    //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                    return true;
                }
            }
            return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
        }
        #endregion
    }
}