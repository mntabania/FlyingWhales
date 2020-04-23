using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeFactionLeader : Interrupt {
        public BecomeFactionLeader() : base(INTERRUPT.Become_Faction_Leader) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            actor.faction.SetLeader(actor);

            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Become Faction Leader", "became_leader");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(actor.faction, actor.faction.name, LOG_IDENTIFIER.FACTION_1);
            return true;
        }
        #endregion
    }
}
