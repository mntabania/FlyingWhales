using System;
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Traits;

namespace Interrupts {
	public class IngestedPoison : Interrupt {
		public IngestedPoison() : base(INTERRUPT.Ingested_Poison) {
			duration = 0;
			isSimulateneous = true;
			interruptIconString = GoapActionStateDB.Sick_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
	        ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
			if (UnityEngine.Random.Range(0, 2) == 0) {
				if(interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Poisoned")) {
                    //TODO: Can still be improved: Create a function that returns the trait that's been added instead of boolean
                    Traits.Poisoned addedPoisoned = interruptHolder.actor.traitContainer.GetNormalTrait<Traits.Poisoned>("Poisoned");
					Traits.Poisoned poisoned = interruptHolder.target.traitContainer.GetNormalTrait<Traits.Poisoned>("Poisoned");
					if (poisoned.responsibleCharacters != null) {
						for (int i = 0; i < poisoned.responsibleCharacters.Count; i++) {
							addedPoisoned.AddCharacterResponsibleForTrait(poisoned.responsibleCharacters[i]);
						}
					}
					
					overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Ingested Poison", "sick", null, logTags);
					overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					//log.AddLogToInvolvedObjects();
				}
			} else {
                interruptHolder.actor.Death("poisoned");
			}

			return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
		}
        public override string ReactionToActor(Character actor, IPointOfInterest target,
	        Character witness,
	        InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            if (!witness.relationshipContainer.IsEnemiesWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            }
            return response;
        }
        #endregion
    }
}