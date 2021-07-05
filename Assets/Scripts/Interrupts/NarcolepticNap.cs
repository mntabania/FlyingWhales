using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class NarcolepticNap : Interrupt {
        public NarcolepticNap() : base(INTERRUPT.Narcoleptic_Nap) {
            duration = 5;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sleep_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Needs};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Resting");
            return true;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Resting");
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            reactions.Add(EMOTION.Shock);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Enemy) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    reactions.Add(EMOTION.Scorn);
                }
            } else if (opinionLabel == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Scorn);
            }
        }
        #endregion
    }
}