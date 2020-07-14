using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeFactionLeader : Interrupt {
        public BecomeFactionLeader() : base(INTERRUPT.Become_Faction_Leader) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.faction.SetLeader(interruptHolder.actor);

            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Become Faction Leader", "became_leader");
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(interruptHolder.actor.faction, interruptHolder.actor.faction.name, LOG_IDENTIFIER.FACTION_1);
            return true;
        }
        #endregion
    }
}
