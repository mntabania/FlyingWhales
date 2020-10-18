using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class RevertFromWerewolf : Interrupt {
        public RevertFromWerewolf() : base(INTERRUPT.Revert_From_Werewolf) {
            duration = 1;
            //isSimulateneous = true;
            shouldStopMovement = false;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            actor.RevertFromWerewolfForm();
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            if (actor.isLycanthrope) {
                actor.lycanData.AddAwareCharacter(witness);
            }

            CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, interrupt.crimeType, interrupt, status);

            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Werewolf);
            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status);
                }
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
                } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status);
                }
                if (target is Character targetCharacter) {
                    string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                    } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                        if (!witness.traitContainer.HasTrait("Psychopath")) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                        }
                    }
                }
            } else {
                if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                    if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
                    }
                } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status);
                }
            }

            return response;
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
