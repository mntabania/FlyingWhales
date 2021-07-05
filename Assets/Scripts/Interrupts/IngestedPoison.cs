using System;
using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
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
				if(interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Poisoned", bypassElementalChance: true)) {
                    Poisoned addedPoisoned = interruptHolder.actor.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
					Poisoned poisoned = interruptHolder.target.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
					if (poisoned.responsibleCharacters != null) {
						for (int i = 0; i < poisoned.responsibleCharacters.Count; i++) {
							addedPoisoned.AddCharacterResponsibleForTrait(poisoned.responsibleCharacters[i]);
						}
					}
					addedPoisoned.SetIsPlayerSource(poisoned.isPlayerSource);
					//if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
					overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Ingested Poison", "sick", null, logTags);
					overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				}
			} else {
                interruptHolder.actor.Death("poisoned");
			}

			return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
		}
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            reactions.Add(EMOTION.Shock);
            if (!witness.relationshipContainer.IsEnemiesWith(actor)) {
                reactions.Add(EMOTION.Concern);
            }
        }
        #endregion
    }
}