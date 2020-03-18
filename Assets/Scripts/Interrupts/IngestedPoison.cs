using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
	public class IngestedPoison : Interrupt {
		public IngestedPoison() : base(INTERRUPT.Ingested_Poison) {
			duration = 0;
			isSimulateneous = true;
			interruptIconString = GoapActionStateDB.Sick_Icon;
		}

		#region Overrides
		public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
			if (UnityEngine.Random.Range(0, 2) == 0) {
				if(actor.traitContainer.AddTrait(actor, "Poisoned")) {
                    //TODO: Can still be improved: Create a function that returns the trait that's been added instead of boolean
                    Traits.Poisoned addedPoisoned = actor.traitContainer.GetNormalTrait<Traits.Poisoned>("Poisoned");
					Traits.Poisoned poisoned = target.traitContainer.GetNormalTrait<Traits.Poisoned>("Poisoned");
					if (poisoned.responsibleCharacters != null) {
						for (int i = 0; i < poisoned.responsibleCharacters.Count; i++) {
							addedPoisoned.AddCharacterResponsibleForTrait(poisoned.responsibleCharacters[i]);
						}
					}
					
					overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Poisoned", "sick");
					overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					//log.AddLogToInvolvedObjects();
				}
			} else {
				actor.Death("poisoned");
			}

			return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
		}
		#endregion
	}
}