using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Locations.Settlements;
using Object_Pools;
namespace Interrupts {
    public class LeaveVillage : Interrupt {
        public LeaveVillage() : base(INTERRUPT.Leave_Village) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            BaseSettlement homeSettlement = actor.homeSettlement;
            if (homeSettlement != null) {
                //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "left", null, LOG_TAG.Major);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
                actor.MigrateHomeStructureTo(null);
            }
            return true;
        }
        #endregion
    }
}