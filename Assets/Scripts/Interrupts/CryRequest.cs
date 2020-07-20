using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class CryRequest : Interrupt {
        public CryRequest() : base(INTERRUPT.Cry_Request) {
            duration = 3;
            doesStopCurrentAction = true;
            //isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            //isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "cry");
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_1);
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
                    }
                }
            } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
                }
            } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
                }
            }
            if(actor != target && witness != target && target is Character targetCharacter) {
                if(actor.relationshipContainer.GetAwarenessState(targetCharacter) == AWARENESS_STATE.Missing) {
                    if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                        witness.jobComponent.TriggerRescueJob(targetCharacter);
                    }
                }
            }
            return response;
        }
        #endregion
    }
}
