using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Flirt : Interrupt {
        public Flirt() : base(INTERRUPT.Flirt) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.nonActionEventsComponent.NormalFlirtCharacter(interruptHolder.target as Character, overrideEffectLog);
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
            if (target is Character targetCharacter) {
                if (target != witness) {
                    bool isActorLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                    bool isTargetLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);

                    if (witness.traitContainer.HasTrait("Hemophobic")) {
                        bool isKnownVampire = false;
                        Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                        isKnownVampire = vampire != null && vampire.DoesCharacterKnowThisVampire(witness);
                        if (isKnownVampire) {
                            reactions.Add(EMOTION.Disgust);
                        }
                    }
                    if (witness.traitContainer.HasTrait("Lycanphobic")) {
                        bool isKnownWerewolf = false;
                        isKnownWerewolf = actor.isLycanthrope && actor.lycanData.DoesCharacterKnowThisLycan(witness);
                        if (isKnownWerewolf) {
                            reactions.Add(EMOTION.Disgust);
                        }
                    }

                    if (isActorLoverOrAffairOfWitness) {
                        reactions.Add(EMOTION.Rage);
                        reactions.Add(EMOTION.Betrayal);
                    } else if (isTargetLoverOrAffairOfWitness) {
                        reactions.Add(EMOTION.Rage);
                        if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                            reactions.Add(EMOTION.Betrayal);
                        }
                    } else {
                        Character loverOfActor = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                        if (loverOfActor != null && loverOfActor != targetCharacter) {
                            reactions.Add(EMOTION.Disapproval);
                            reactions.Add(EMOTION.Disgust);
                        } else if (witness.relationshipContainer.IsFriendsWith(actor)) {
                            reactions.Add(EMOTION.Scorn);
                        }
                    }
                } else {
                    //target is witness
                    if (status == REACTION_STATUS.INFORMED) {
                        reactions.Add(EMOTION.Embarassment);
                    }
                }
            }
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            if (target is Character targetCharacter) {
                if ((actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false && actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))
                    || (targetCharacter.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER) == false && targetCharacter.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))) {
                    return CRIME_TYPE.Infidelity;
                }
            }
            return base.GetCrimeType(actor, target, crime);
        }
        #endregion
    }
}