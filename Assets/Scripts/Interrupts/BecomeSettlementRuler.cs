using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeSettlementRuler : Interrupt {
        public BecomeSettlementRuler() : base(INTERRUPT.Become_Settlement_Ruler) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.homeSettlement.SetRuler(actor);

            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Become NPCSettlement Ruler", "became_ruler");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(actor.homeSettlement, actor.homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            PlayerManager.Instance.player.ShowNotificationFrom(actor, overrideEffectLog);
            return true;
        }
        #endregion
    }
}