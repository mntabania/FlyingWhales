using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class Puke : Interrupt {
        public Puke() : base(INTERRUPT.Puke) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sick_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.SetPOIState(POI_STATE.INACTIVE);
            return true;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.SetPOIState(POI_STATE.ACTIVE);
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
            if (actor.homeSettlement is NPCSettlement npcSettlement && npcSettlement.isPlagued) {
                if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    if (GameUtilities.RollChance(15)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Plague_Hysteria, witness, actor, status);
                    }
                } else if (witness.relationshipContainer.HasRelationshipWith(actor) == false || 
                           witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, BaseRelationshipContainer.Acquaintance)) {
                    if (GameUtilities.RollChance(40)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Plague_Hysteria, witness, actor, status);
                    }
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Plague_Hysteria, witness, actor, status);
                }
            }
            return response;
        }
        #endregion
    }
}