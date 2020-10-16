using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BurningAtStake : Interrupt {
        public BurningAtStake() : base(INTERRUPT.Burning_At_Stake) {
            duration = 36;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            shouldEndOnSeize = true;
            interruptIconString = GoapActionStateDB.Cowering_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Burning At Stake");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool PerTickInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.AdjustHP(-100, ELEMENTAL_TYPE.Fire);
            if(interruptHolder.actor.currentHP <= 0) {
                interruptHolder.actor.Death(cause: "burn_at_stake");
            }
            return true;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Burning At Stake");
            if (!interruptHolder.actor.isDead) {
                interruptHolder.actor.AdjustHP(-500, ELEMENTAL_TYPE.Fire);
                if (interruptHolder.actor.currentHP <= 0) {
                    interruptHolder.actor.Death(cause: "burn_at_stake");
                }
            }
            return true;
        }
        public override bool OnForceEndInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Burning At Stake");
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Sadness, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel != RelationshipManager.Rival &&
                       (witness.relationshipContainer.IsFamilyMember(actor) ||
                        witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER))) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            }
            return response;
        }
        #endregion
    }
}