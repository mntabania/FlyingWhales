using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Object_Pools;
namespace Interrupts {
    public class LeaveHome : Interrupt {
        public LeaveHome() : base(INTERRUPT.Leave_Home) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            LocationStructure homeStructure = actor.homeStructure;
            if(homeStructure != null) {
                //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "left", null, LOG_TAG.Major);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(homeStructure, homeStructure.name, LOG_IDENTIFIER.LANDMARK_1);
                actor.MigrateHomeStructureTo(null, affectSettlement: false);
            }
            return true;
        }
        #endregion
    }
}