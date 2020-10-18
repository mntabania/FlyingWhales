using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class KnockoutCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public KnockoutCharacter() : base(INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Stealth_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
            RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
            RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL, 
            RACE.TROLL };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Combat};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Knockout Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target is Character) {
            Character targetCharacter = target as Character;
            string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                cost += 35;
                costLog += " +35(Friend/Close Friend)";
            } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                cost += 0;
                costLog += $" +0(Enemy/Rival)";
            } else if (opinionLabel == RelationshipManager.Acquaintance || actor.faction == targetCharacter.faction) {
                cost += 20;
                costLog += $" +20(Acquaintance/Same Faction)";
            } else {
                cost += 10;
                costLog += " +10(Else)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (node.crimeType == CRIME_TYPE.Vampire) {
                CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType);
                if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                    if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    } else if (opinionOfTarget == RelationshipManager.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                            if (GameUtilities.RollChance(chance)) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);        
                            }
                        }
                    } else if (opinionOfTarget == RelationshipManager.Friend ||opinionOfTarget == RelationshipManager.Acquaintance) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    } else if (targetCharacter == witness) {
                        CharacterManager.Instance.TriggerEmotion(GameUtilities.RollChance(50) ? EMOTION.Anger : EMOTION.Resentment, witness, actor, status, node);
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Hemophiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);
                        } else {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                        }
                    } else if (witness.traitContainer.HasTrait("Hemophobic")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    }
                }
            } else if (node.crimeType == CRIME_TYPE.Assault) {
                if (opinionOfTarget == RelationshipManager.Rival) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                } else {
                    if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    } else if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    } else if (opinionOfTarget == RelationshipManager.Acquaintance) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                    } else if (targetCharacter == witness) {
                        CharacterManager.Instance.TriggerEmotion(GameUtilities.RollChance(50) ? EMOTION.Anger : EMOTION.Resentment, witness, actor, status, node);
                    }
                }
            }
            
            

            if (node.associatedJobType != JOB_TYPE.APPREHEND || node.associatedJobType != JOB_TYPE.RESTRAIN) {
                if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                    //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_SEVERITY.Misdemeanor);
                    CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        if (target is Character targetCharacter) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
            if (targetCharacter.traitContainer.HasTrait("Hothead")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, targetCharacter, actor, status, node);
            }
            if (node.associatedJobType != JOB_TYPE.APPREHEND || node.associatedJobType != JOB_TYPE.RESTRAIN) {
                if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                    CrimeManager.Instance.ReactToCrime(targetCharacter, actor, target, target.factionOwner, node.crimeType, node, status);
                }
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(target is Character targetCharacter) {
            if (targetCharacter.race.IsSapient()) {
                if (crime.associatedJobType != JOB_TYPE.APPREHEND && crime.associatedJobType != JOB_TYPE.RESTRAIN) {
                    //since there is no drink blood job (it uses fullness recovery), to check if job is from drink blood, just check if the associated job type is knockout
                    //since knockout will only ever be used for fullness recovery if it is for Drinking Blood 
                    bool isDrinkBloodJob = crime.associatedJobType.IsFullnessRecovery(); 
                    if (isDrinkBloodJob || crime.associatedJobType == JOB_TYPE.IMPRISON_BLOOD_SOURCE) {
                        return CRIME_TYPE.Vampire;
                    } else {
                        return CRIME_TYPE.Assault;
                    }
                }
            }
            // if ((targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) && (crime.associatedJobType != JOB_TYPE.APPREHEND || crime.associatedJobType != JOB_TYPE.RESTRAIN)) {
            //     return CRIME_TYPE.Assault;
            // }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && (actor.traitContainer.HasTrait("Psychopath") || actor.traitContainer.HasTrait("Vampire") || actor.isNormalCharacter == false);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterKnockoutSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Unconscious", goapNode.actor, gainedFromDoing: goapNode);
    }
    //public void PreKnockoutFail() {
    //    SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
    //    currentState.SetIntelReaction(KnockoutFailIntelReaction);
    //}
    //public void AfterKnockoutFail() {
    //    if(poiTarget is Character) {
    //        Character targetCharacter = poiTarget as Character;
    //        if (!targetCharacter.ReactToCrime(committedCrime, this, actorAlterEgo, SHARE_INTEL_STATUS.WITNESSED)) {
    //            RelationshipManager.Instance.RelationshipDegradation(actor, targetCharacter, this);
    //            targetCharacter.combatComponent.AddHostileInRange(actor, false);
    //            //NOTE: Adding hostile in range is done after the action is done processing fully, See OnResultReturnedToActor
    //        }
    //    }
    //}
    #endregion

    //#region Intel Reactions
    //private List<string> KnockoutSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("Do not tell anybody, please!");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                reactions.Add(string.Format("I'm embarrassed that {0} was able to do that to me!", actor.name));
    //            }
    //            //- Recipient Has Positive Relationship with Actor
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateAttemptToStopCurrentActionAndJob(actor, parentPlan.job);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
    //                } else {
    //                    reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Actor
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
    //                    }
    //                    reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add("My enemies fighting each other. What a happy day!");
    //                } else {
    //                    recipient.ReactToCrime(CRIME.ABERRATION, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
    //                    }
    //                    reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
    //                }
    //            }
    //            //- Recipient Has No Relationship with Actor
    //            else {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else {
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //private List<string> KnockoutFailIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("Do not tell anybody, please!");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                reactions.Add(string.Format("{0} failed. Anyone that tries to do that will also fail.", actor.name));
    //            }
    //            //- Recipient Has Positive Relationship with Actor
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        //Add Attempt to Stop Job
    //                        //recipient.CreateKnockoutJob(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
    //                } else {
    //                    reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Actor
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
    //                    }
    //                    reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add("My enemies fighting each other. What a happy day!");
    //                } else {
    //                    recipient.ReactToCrime(CRIME.ABERRATION, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
    //                    }
    //                    reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
    //                }
    //            }
    //            //- Recipient Has No Relationship with Actor
    //            else {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else {
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}