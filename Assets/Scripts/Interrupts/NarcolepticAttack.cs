using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class NarcolepticAttack : Interrupt {
        public NarcolepticAttack() : base(INTERRUPT.Narcoleptic_Attack) {
            duration = 6;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            actor.traitContainer.AddTrait(actor, "Resting");
            return true;
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.traitContainer.RemoveTrait(actor, "Resting");
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Enemy) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
                }
            } else if (opinionLabel == RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
            }
            return response;
        }
        #endregion
    }
}