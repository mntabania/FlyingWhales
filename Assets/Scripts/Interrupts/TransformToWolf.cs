using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Crime_System;
using Logs;
using UtilityScripts;
namespace Interrupts {
    public class TransformToWolf : Interrupt {
        public TransformToWolf() : base(INTERRUPT.Transform_To_Wolf) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Lycan_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Transforming");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            Character actor = interruptHolder.actor;
            if(actor.marker && actor.currentRegion != null) {
                //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, actor.marker.transform.position, 1, actor.currentRegion.innerMap);
            }
            if (actor.isLycanthrope) {
                actor.lycanData.TurnToWolf();
            } else {
                actor.traitContainer.RemoveTrait(actor, "Transforming");
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override bool OnForceEndInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Transforming");
            return base.OnForceEndInterrupt(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            if(actor.isLycanthrope) {
                actor.lycanData.AddAwareCharacter(witness);
            }
            return response;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            Character originalForm = actor;
            if (actor.isLycanthrope) {
                originalForm = actor.lycanData.originalForm;
            }
            if (!witness.isLycanthrope) {
                CrimeType crimeTypeObj = CrimeManager.Instance.GetCrimeType(interrupt.crimeType);
                CRIME_SEVERITY severity = CRIME_SEVERITY.None;
                if (crimeTypeObj != null) {
                    severity = CrimeManager.Instance.GetCrimeSeverity(witness, originalForm, target, interrupt.crimeType);
                }
                if (severity == CRIME_SEVERITY.Heinous) {
                    // response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm);
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(originalForm);
                    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Despair);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(originalForm)) {
                        reactions.Add(EMOTION.Despair);
                    }
                    if (witness.traitContainer.HasTrait("Coward")) {
                        reactions.Add(EMOTION.Fear);
                    } else {
                        reactions.Add(EMOTION.Threatened);
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, originalForm)) {
                            reactions.Add(EMOTION.Arousal);
                        } else {
                            reactions.Add(EMOTION.Approval);
                        }
                    } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                        reactions.Add(EMOTION.Threatened);
                    }
                }

                //CrimeManager.Instance.ReactToCrime(witness, originalForm, target, target.factionOwner, interrupt.crimeType, interrupt, status);
            }
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
