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
            interruptIconString = GoapActionStateDB.Injured_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            if(interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Injured")) {
                return true;
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance) {
                if(UnityEngine.Random.Range(0, 2) == 0) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
                }
            } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel == RelationshipManager.Enemy) {
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

