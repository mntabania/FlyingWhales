using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BurningAtStake : Interrupt {
        public BurningAtStake() : base(INTERRUPT.Burning_At_Stake) {
            duration = 20;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            shouldEndOnSeize = true;
            interruptIconString = GoapActionStateDB.Burn_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        //NOTE: The actor in this interrupt is the one being burned at stake, while the target is the one that burned the actor
        //Normally we will also set the actor as the target in this kind of situation, but since we need to know who is the responsible character for the actor's burning at stake, we need a way to pass the responsible charater to this interrupt
        //And the way to do it is set that responsible character as the target
        //The reason why we need to pass the responsible character is for the Dead trait to know who's responsible for the death of the one being burned at stake
        //It might be confusing, hence, this note
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Burning At Stake");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool PerTickInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.AdjustHP(-500, ELEMENTAL_TYPE.Fire);
            if(!interruptHolder.actor.HasHealth()) {
                interruptHolder.actor.Death(cause: "burn_at_stake", responsibleCharacter: interruptHolder.target as Character);
            }
            return true;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Burning At Stake");
            if (!interruptHolder.actor.isDead) {
                interruptHolder.actor.AdjustHP(-500, ELEMENTAL_TYPE.Fire);
                if (!interruptHolder.actor.HasHealth()) {
                    interruptHolder.actor.Death(cause: "burn_at_stake", responsibleCharacter: interruptHolder.target as Character);
                }
            }
            return true;
        }
        public override bool OnForceEndInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Burning At Stake");
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Sadness);
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel != RelationshipManager.Rival &&
                       (witness.relationshipContainer.IsFamilyMember(actor) ||
                        witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER))) {
                reactions.Add(EMOTION.Concern);
                reactions.Add(EMOTION.Despair);
            } else {
                reactions.Add(EMOTION.Shock);
            }
        }
        #endregion
    }
}