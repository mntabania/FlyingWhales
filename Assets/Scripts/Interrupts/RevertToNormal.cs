using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Crime_System;

namespace Interrupts {
    public class RevertToNormal : Interrupt {
        public RevertToNormal() : base(INTERRUPT.Revert_To_Normal) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Transforming");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.lycanData.RevertToNormal();
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            Character originalForm = actor.lycanData.originalForm;
            if (!witness.isLycanthrope) {
                CrimeType crimeTypeObj = CrimeManager.Instance.GetCrimeType(interrupt.crimeType);
                CRIME_SEVERITY severity = CRIME_SEVERITY.None;
                if (crimeTypeObj != null) {
                    severity = CrimeManager.Instance.GetCrimeSeverity(witness, originalForm, target, interrupt.crimeType, interrupt);
                }
                if (severity == CRIME_SEVERITY.Heinous) {
                    // response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm);
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(originalForm);
                    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                        opinionLabel == RelationshipManager.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                    } else if ((witness.relationshipContainer.IsFamilyMember(originalForm) || witness.relationshipContainer.HasRelationshipWith(originalForm, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                    }
                    if (witness.traitContainer.HasTrait("Coward")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, originalForm, status);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm, status);
                    }
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, originalForm, status);
                }

                //CrimeManager.Instance.ReactToCrime(witness, originalForm, this, CRIME_SEVERITY.Heinous);
                CrimeManager.Instance.ReactToCrime(witness, originalForm, target, target.factionOwner, interrupt.crimeType, interrupt, status);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, originalForm, status);
            }
            return response;
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
