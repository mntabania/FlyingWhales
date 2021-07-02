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
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
            RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
            RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL, 
            RACE.TROLL, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Combat};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Knockout Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        int cost = 0;
        if (target is Character) {
            Character targetCharacter = target as Character;
            string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                cost += 35;
#if DEBUG_LOG
                costLog += " +35(Friend/Close Friend)";
#endif
            } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                cost += 0;
#if DEBUG_LOG
                costLog += $" +0(Enemy/Rival)";
#endif
            } else if (opinionLabel == RelationshipManager.Acquaintance || actor.faction == targetCharacter.faction) {
                cost += 20;
#if DEBUG_LOG
                costLog += $" +20(Acquaintance/Same Faction)";
#endif
            } else {
                cost += 10;
#if DEBUG_LOG
                costLog += " +10(Else)";
#endif
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (node.crimeType == CRIME_TYPE.Vampire) {
                CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType);
                if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                    if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        reactions.Add(EMOTION.Rage);
                        reactions.Add(EMOTION.Threatened);
                    } else if (opinionOfTarget == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                        reactions.Add(EMOTION.Threatened);
                    } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                        reactions.Add(EMOTION.Approval);

                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                            if (GameUtilities.RollChance(chance)) {
                                reactions.Add(EMOTION.Arousal);
                            }
                        }
                    } else if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Acquaintance) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Threatened);
                    } else if (targetCharacter == witness) {
                        if (GameUtilities.RollChance(50)) {
                            reactions.Add(EMOTION.Anger);
                        } else {
                            reactions.Add(EMOTION.Resentment);
                        }
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Hemophiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            reactions.Add(EMOTION.Arousal);
                        } else {
                            reactions.Add(EMOTION.Approval);
                        }
                    } else if (witness.traitContainer.HasTrait("Hemophobic")) {
                        reactions.Add(EMOTION.Threatened);
                    }
                }
            } else if (node.crimeType == CRIME_TYPE.Assault) {
                if (opinionOfTarget == RelationshipManager.Rival) {
                    reactions.Add(EMOTION.Approval);
                } else {
                    if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        reactions.Add(EMOTION.Rage);
                        reactions.Add(EMOTION.Threatened);
                    } else if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                        reactions.Add(EMOTION.Threatened);
                    } else if (opinionOfTarget == RelationshipManager.Acquaintance) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Threatened);
                    } else if (targetCharacter == witness) {
                        if (GameUtilities.RollChance(50)) {
                            reactions.Add(EMOTION.Anger);
                        } else {
                            reactions.Add(EMOTION.Resentment);
                        }
                    }
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            reactions.Add(EMOTION.Threatened);
            if (targetCharacter.traitContainer.HasTrait("Hothead")) {
                reactions.Add(EMOTION.Rage);
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if (crime.associatedJobType == JOB_TYPE.SNATCH) {
            //Crime is Assault if the job is Snatch because Snatch jobs are supposed to be Stealth, and Stealth only works if the action is a crime
            return CRIME_TYPE.Assault;
        }
        if (target is Character targetCharacter) {
            if (targetCharacter.race.IsSapient()) {
                if (crime.associatedJobType != JOB_TYPE.APPREHEND && crime.associatedJobType != JOB_TYPE.RESTRAIN) {
                    //since there is no drink blood job (it uses fullness recovery), to check if job is from drink blood, just check if the associated job type is knockout
                    //since knockout will only ever be used for fullness recovery if it is for Drinking Blood 
                    bool isDrinkBloodJob = crime.associatedJobType.IsFullnessRecoveryTypeJob(); 
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (actor == poiTarget) {
                return false;
            }
            if (poiTarget.traitContainer.HasTrait("Sturdy")) {
                //Cannot knock out sturdy characters
                return false;
            }
            //if (job != null && job.jobType == JOB_TYPE.SNATCH) { //&& actor.traitContainer.HasTrait("Cultist")
            //    return true; //only allow cultists to use knock out if it is for snatching 
            //}
            if (actor.race == RACE.TRITON) {
                return true;
            }
            if (!actor.isNormalCharacter) {
                //Monsters, minions and ratmen cannot do knockout character
                return false;
            }
            return actor.traitContainer.HasTrait("Psychopath", "Vampire");
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterKnockoutSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Unconscious", goapNode.actor);
        goapNode.poiTarget.traitContainer.GetTraitOrStatus<Trait>("Unconscious")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
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