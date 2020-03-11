using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
	public class Plagued : Interrupt {
		public Plagued() : base(INTERRUPT.Plagued) {
			duration = 0;
			isSimulateneous = true;
			interruptIconString = GoapActionStateDB.Flirt_Icon;
		}

		#region Overrides
		public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
			if (actor.traitContainer.AddTrait(actor, "Plagued")) {
				overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Plagued", "contract");
				overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				//log.AddLogToInvolvedObjects();
				return true;
			}
			return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
		}
		#endregion
	}
}