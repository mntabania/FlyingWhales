using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Flirt : Interrupt {
        public Flirt() : base(INTERRUPT.Flirt) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.nonActionEventsComponent.NormalFlirtCharacter(interruptHolder.target as Character, ref overrideEffectLog);
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            if(target != witness && target is Character targetCharacter) {
                bool isActorLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                bool isTargetLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);

                if (isActorLoverOrAffairOfWitness) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                } else if (isTargetLoverOrAffairOfWitness) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                } else {
                    //TODO: Positive/Negative Result
                }
            }
            return response;
        }
        #endregion
    }
}