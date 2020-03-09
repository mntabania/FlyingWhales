using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class KnockoutCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public KnockoutCharacter() : base(INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Stealth_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Knockout Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target is Character) {
            Character targetCharacter = target as Character;
            string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend || opinionLabel == OpinionComponent.Acquaintance
                || actor.faction == targetCharacter.faction || actor.homeSettlement == targetCharacter.homeSettlement) {
                cost += 15;
                costLog += " +15(Friend/Close/Acquaintance/Same Faction/NPCSettlement)";
            } else if (job.jobType == JOB_TYPE.UNDERMINE) {
                cost += UtilityScripts.Utilities.rng.Next(80, 121);
                costLog += $" +{cost}(Undermine)";
            } else {
                cost += 2000;
                costLog += " +2000(Else)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (opinionLabel == OpinionComponent.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
            } else {
                if(node.associatedJobType != JOB_TYPE.APPREHEND) {
                    if (witness.homeSettlement == targetCharacter.homeSettlement || witness.faction == targetCharacter.faction
                        || witness.relationshipContainer.HasRelationshipWith(targetCharacter)) {
                        CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
                    }
                }
            }

            if (opinionLabel == OpinionComponent.Acquaintance) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
            } else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
            }
        }
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && (actor.traitContainer.HasTrait("Psychopath") || actor.traitContainer.HasTrait("Vampiric"));
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreKnockoutSuccess(ActualGoapNode goapNode) {
        //TODO: currentState.SetIntelReaction(KnockoutSuccessIntelReaction);
    }
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

public class KnockoutCharacterData : GoapActionData {
    public KnockoutCharacterData() : base(INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && actor.traitContainer.HasTrait("Psychopath");
    }
}
