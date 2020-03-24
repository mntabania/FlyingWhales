using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RestrainCharacter : GoapAction {

    public RestrainCharacter() : base(INTERACTION_TYPE.RESTRAIN_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Restrain_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON, RACE.SPIDER };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, CannotMove);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        //TODO: isForCriminal = parentPlan != null && parentPlan.job != null && (parentPlan.job.jobType == JOB_TYPE.APPREHEND || parentPlan.job.jobType == JOB_TYPE.RESTRAIN);
        SetState("Restrain Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character target = poiTarget as Character;
            if (target.IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if(target is Character) {
            Character targetCharacter = target as Character;
            if (targetCharacter.traitContainer.HasTrait("Criminal")) {
                if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Sadness, witness, actor, status);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
                }
            } else {
                if (!witness.relationshipContainer.IsEnemiesWith(targetCharacter) && !witness.IsHostileWith(targetCharacter)) {
                    CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
                    if (!witness.traitContainer.HasTrait("Psychopath") && witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status);
                        if(UnityEngine.Random.Range(0, 100) < 20) {
                            if (!witness.traitContainer.HasTrait("Diplomatic")) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                            }
                        }
                    }
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (targetCharacter.traitContainer.HasTrait("Criminal")) {
                if (!witness.traitContainer.HasTrait("Psychopath") && witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status);
                } else if (UnityEngine.Random.Range(0, 100) < 30 && !witness.traitContainer.HasTrait("Diplomatic")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, targetCharacter, status);
                }
            } else {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if(opinionLabel == OpinionComponent.Acquaintance) {
                    if (!witness.traitContainer.HasTrait("Psychopath") && UnityEngine.Random.Range(0, 2) == 0) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status);
                    }
                } else if (opinionLabel == OpinionComponent.Enemy || opinionLabel == OpinionComponent.Rival) {
                    if (!witness.traitContainer.HasTrait("Diplomatic")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, targetCharacter, status);
                    }
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionOfTarget(node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (!targetCharacter.IsHostileWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, targetCharacter, actor, status);
                if (targetCharacter.traitContainer.HasTrait("Hothead") || UnityEngine.Random.Range(0, 100) < 35) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, targetCharacter, actor, status);
                }
            }
        }
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor != poiTarget) {
                Character target = poiTarget as Character;
                return !target.traitContainer.HasTrait("Restrained");
            }
            return false;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRestrainSuccess(ActualGoapNode goapNode) {
        //TODO: currentState.SetIntelReaction(SuccessReactions);
    }
    public void AfterRestrainSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Target gains Restrained trait.
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Restrained", goapNode.actor);
    }
    #endregion

    #region Preconditions
    private bool CannotMove(Character actor, IPointOfInterest target, object[] otherData) {
        return target.traitContainer.HasTrait("Unconscious");
    }
    #endregion

    //#region Intel Reactions
    //private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;

    //    //If to imprison a criminal:
    //    if (isForCriminal) {
    //        if (recipient == actor) {
    //            //-Is Actor
    //            if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                //- If Informed: "[Target Name] did something wrong."
    //                reactions.Add(string.Format("{0} did something wrong.", target.name));
    //            }
    //        } else if (recipient == target) {
    //            //- Is Target
    //            if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                //- If Informed: "I got caught."
    //                reactions.Add("I got caught.");
    //            }
    //        } else {
    //            //- Otherwise:
    //            if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                //-If Informed: "If you do something bad here, you get imprisoned. That's the law."
    //                reactions.Add("If you do something bad here, you get imprisoned. That's the law.");
    //            }
    //        }
    //    }
    //    //Otherwise (usually criminal stuff like Serial Killing):
    //    else {
    //        RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //        RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(target.currentAlterEgo);
    //        if (recipient == actor) {
    //            if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                //- If Informed: "Do not tell anybody, please!"
    //                reactions.Add("Do not tell anybody, please!");
    //            }
    //        } else if (recipient == target) {
    //            if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                // - If Informed: "That was a traumatic experience."
    //                reactions.Add("That was a traumatic experience.");
    //            }
    //        } else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //            if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                //- Considers it an Assault
    //                recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED && actor.currentAction != null && actor.currentAction.parentPlan != null && actor.currentAction.parentPlan.job != null) {
    //                    //-If witnessed: Add Attempt to Stop Job targeting Actor
    //                    recipient.CreateAttemptToStopCurrentActionAndJob(target, actor.currentAction.parentPlan.job);
    //                }
    //                if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    //- If informed: "[Actor Name] shouldn't have done that to [Target Name]!"
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, target.name));
    //                }
    //            } else if (relWithTarget == RELATIONSHIP_EFFECT.NONE) {
    //                if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    // - If informed: "I'm sure there's a reason [Actor Name] did that."
    //                    reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
    //                }
    //            } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    // - If informed: "I'm sure there's a reason [Actor Name] did that."
    //                    reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
    //                }
    //            }
    //        } else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
    //            if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                //- Considers it an Assault
    //                recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //- If witnessed: Add Assault Job targeting Actor
    //                    recipient.CreateKnockoutJob(actor);
    //                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    //- If informed: "[Actor Name] shouldn't have done that to [Target Name]!"
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, target.name));
    //                }
    //            } else if (relWithTarget == RELATIONSHIP_EFFECT.NONE) {
    //                RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                //- Considers it an Assault
    //                recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //- If witnessed: Temporarily add Actor to Avoid List
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    //- If informed: "[Actor Name] shouldn't have done that to [Target Name]!"
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, target.name));
    //                }
    //            } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //- If witnessed: Temporarily add Actor to Avoid List
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    //- If informed: "I am not fond of [Target Name] at all so I don't care what happens to [him/her]."
    //                    reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                }
    //            }
    //        } else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //            if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                //- Considers it an Assault
    //                recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //- If witnessed: Add Assault Job targeting Actor
    //                    recipient.CreateKnockoutJob(actor);
    //                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    // - If informed:  Add Undermine Job targeting Actor
    //                    recipient.CreateUndermineJobOnly(actor, "informed");
    //                    //- If informed: "[Actor Name] is such a vile creature!"
    //                    reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
    //                }
    //            } else if (relWithTarget == RELATIONSHIP_EFFECT.NONE) {
    //                RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                //- Considers it Aberration
    //                recipient.ReactToCrime(CRIME.ABERRATION, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //- If witnessed: Add Assault Job targeting Actor
    //                    recipient.CreateKnockoutJob(actor);
    //                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    // - If informed:  Add Undermine Job targeting Actor
    //                    recipient.CreateUndermineJobOnly(actor, "informed");
    //                    //- If informed: "[Actor Name] is such a vile creature!"
    //                    reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
    //                }
    //            } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                //- Considers it an Assault
    //                recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //- If witnessed: Temporarily add Actor to Avoid List
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                    //- If informed: "My enemies fighting each other. What a happy day!"
    //                    reactions.Add("My enemies fighting each other. What a happy day!");
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class RestrainCharacterData : GoapActionData {
    public RestrainCharacterData() : base(INTERACTION_TYPE.RESTRAIN_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            Character target = poiTarget as Character;
            return !target.traitContainer.HasTrait("Restrained");
        }
        return false;
    }
}
