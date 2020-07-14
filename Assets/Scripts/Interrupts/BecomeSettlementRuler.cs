using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeSettlementRuler : Interrupt {
        public BecomeSettlementRuler() : base(INTERRUPT.Become_Settlement_Ruler) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            actor.homeSettlement.SetRuler(actor);

            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Become Settlement Ruler", "became_ruler");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(actor.homeSettlement, actor.homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            return true;
        }
        #endregion
    }
}