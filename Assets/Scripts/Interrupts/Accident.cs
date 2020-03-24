using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Accident : Interrupt {
        public Accident() : base(INTERRUPT.Accident) {
            duration = 1;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            if(actor.traitContainer.AddTrait(actor, "Injured")) {
                return true;
            }
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == OpinionComponent.Acquaintance) {
                if(UnityEngine.Random.Range(0, 2) == 0) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
                }
            } else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel == OpinionComponent.Enemy) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
                }
            } else if (opinionLabel == OpinionComponent.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
            }
            return response;
        }
        #endregion
    }
}

