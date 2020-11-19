using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Plague.Transmission;

namespace Interrupts {
    public class Sneeze : Interrupt {
        public Sneeze() : base(INTERRUPT.Sneeze) {
            duration = 0;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            isSimulateneous = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            if (actor.traitContainer.HasTrait("Plagued")) {
                AirborneTransmission.Instance.Transmit(actor, null, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
                return true;
            }
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        //public override string ReactionToActor(Character actor, IPointOfInterest target,
        //    Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
        //    string response = base.ReactionToActor(actor, target, witness, interrupt, status);

        //    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);

        //    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
        //    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
        //        opinionLabel == RelationshipManager.Close_Friend) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
        //    } else if ((witness.relationshipContainer.IsFamilyMember(actor) || 
        //                witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) && 
        //               !witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Rival)) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
        //    } else if (opinionLabel == RelationshipManager.Rival) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
        //    }
        //    if (witness.traitContainer.HasTrait("Coward")) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
        //    }

        //    if (status == REACTION_STATUS.WITNESSED && actor.homeSettlement != null && actor.homeSettlement is NPCSettlement settlement) {
        //        //When a resident has been witnessed to die due to Septic Shock, the Settlement will be flagged as Plagued
        //        settlement.SetIsPlagued(true);
        //    }

        //    return response;
        //}
        #endregion
    }
}