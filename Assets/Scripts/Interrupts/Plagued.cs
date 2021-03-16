using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;

namespace Interrupts {
	public class Plagued : Interrupt {
		public Plagued() : base(INTERRUPT.Plagued) {
			duration = 0;
			isSimulateneous = true;
			interruptIconString = GoapActionStateDB.No_Icon;
			logTags = new[] {LOG_TAG.Life_Changes};
		}

		#region Overrides
		public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
			if (PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(interruptHolder.actor)) {
				//if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
				overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Plagued", "contract", null, logTags);
				overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				return true;
			}
			return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
		}
		#endregion
	}
}