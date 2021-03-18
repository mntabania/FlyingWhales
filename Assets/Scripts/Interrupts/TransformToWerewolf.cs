using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class TransformToWerewolf : Interrupt {
        public TransformToWerewolf() : base(INTERRUPT.Transform_To_Werewolf) {
            duration = 1;
            //isSimulateneous = true;
            shouldStopMovement = false;
            interruptIconString = GoapActionStateDB.Lycan_Icon;
            logTags = new[] {LOG_TAG.Combat, LOG_TAG.Crimes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, actor.marker.transform.position, 2, actor.currentRegion.innerMap);
            actor.RevertFromVampireBatForm();
            actor.TransformToWerewolfForm();
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            if(actor.isLycanthrope) {
                actor.lycanData.AddAwareCharacter(witness);
            }

            //CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, interrupt.crimeType, interrupt, status);
            return response;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Werewolf);
            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                reactions.Add(EMOTION.Shock);
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Despair);
                }
                if (witness.traitContainer.HasTrait("Coward")) {
                    reactions.Add(EMOTION.Fear);
                } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Threatened);
                }
                if (target is Character targetCharacter) {
                    string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                    } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                        if (!witness.traitContainer.HasTrait("Psychopath")) {
                            reactions.Add(EMOTION.Anger);
                        }
                    }
                }
            } else {
                if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                    if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                        reactions.Add(EMOTION.Arousal);
                    } else {
                        reactions.Add(EMOTION.Approval);
                    }
                } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                    reactions.Add(EMOTION.Threatened);
                }
            }
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
