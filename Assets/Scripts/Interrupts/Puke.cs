﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class Puke : Interrupt {
        public Puke() : base(INTERRUPT.Puke) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sick_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Needs};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.SetPOIState(POI_STATE.INACTIVE);
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(interruptHolder.target, interruptHolder.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_1);
            if (GameUtilities.RollChance(5) && interruptHolder.actor.homeSettlement != null && 
                Locations.Settlements.Settlement_Events.Plagued.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement)) {
                interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued);
            }
            return true;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.SetPOIState(POI_STATE.ACTIVE);
            return true;
        }
        public override bool PerTickInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.needsComponent.AdjustFullness(-1);
            return base.PerTickInterrupt(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel != RelationshipManager.Rival && 
                       (witness.relationshipContainer.IsFamilyMember(actor) || 
                        witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER))) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
            }

            if (actor.homeSettlement is NPCSettlement npcSettlement && npcSettlement.isPlagued) {
                if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    if (GameUtilities.RollChance(15)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Plague_Hysteria, witness, actor, status);
                    }
                } else if ((witness.relationshipContainer.IsFamilyMember(actor) || 
                           witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) && 
                           !witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Rival)) {
                    // if Actor is Relative, Lover, Affair and not a Rival
                    if (GameUtilities.RollChance(15)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Plague_Hysteria, witness, actor, status);
                    }
                } else if (witness.relationshipContainer.HasRelationshipWith(actor) == false || 
                          witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Acquaintance)) {
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