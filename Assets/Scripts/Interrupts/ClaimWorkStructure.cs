using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;
using Object_Pools;

namespace Interrupts {
    public class ClaimWorkStructure : Interrupt {
        public ClaimWorkStructure() : base(INTERRUPT.Claim_Work_Structure) {
            duration = 0;
            interruptIconString = GoapActionStateDB.No_Icon;
            isSimulateneous = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            //NOTE: YOU MUST SET THE WORK STRUCTURE FIRST BEFORE CALLING THIS INTERRUPT
            //This is only for logging purposes
            //This is just a workaround because we can't target structures in interrupts
            ManMadeStructure currentWorkStructure = actor.structureComponent.workPlaceStructure;
            if (currentWorkStructure != null) {
                if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "set_work_structure", null, logTags);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(currentWorkStructure, currentWorkStructure.name, LOG_IDENTIFIER.LANDMARK_1);
            }
            return true;
        }
        #endregion
    }
}