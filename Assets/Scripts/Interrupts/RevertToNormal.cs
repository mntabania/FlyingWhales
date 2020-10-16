﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Crime_System;
using Logs;
using UtilityScripts;
namespace Interrupts {
    public class RevertToNormal : Interrupt {
        public RevertToNormal() : base(INTERRUPT.Revert_To_Normal) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Transforming");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.lycanData.RevertToNormal();
            if (!interruptHolder.actor.lycanData.isMaster && GameUtilities.RollChance(25)) { //25
                //chance to master
                interruptHolder.actor.lycanData.SetIsMaster(true);
                Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Revert To Normal", "mastered");
                log.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            Character originalForm = actor.lycanData.originalForm;
            
            actor.lycanData.AddAwareCharacter(witness);

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
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(originalForm)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                    }
                    if (witness.traitContainer.HasTrait("Coward")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, originalForm, status);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm, status);
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, originalForm)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, originalForm, status);
                        } else {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, originalForm, status);
                        }
                    } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm, status);
                    }
                }

                //Remove this temporarily because interrupt holders are object pooled, so when you report a crime that is an interrupt and that interrupt is already claimed by the object pool
                //it will create null exceptions since the data inside is already cleared out
                //CrimeManager.Instance.ReactToCrime(witness, originalForm, target, target.factionOwner, interrupt.crimeType, interrupt, status);
            }
            return response;
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
