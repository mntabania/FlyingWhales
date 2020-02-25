using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class SlayCharacter : GoapAction {
    public SlayCharacter() : base(INTERACTION_TYPE.SLAY_CHARACTER) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), TargetCannotMove);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Slay Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).isDead == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    //public override int GetArrangedLogPriorityIndex(string priorityID) {
    //    if (priorityID == "description") {
    //        return 0;
    //    } else if (priorityID == "unconscious") {
    //        return 1;
    //    }
    //    return base.GetArrangedLogPriorityIndex(priorityID);
    //}
    //public override void OnResultReturnedToActor() {
    //    base.OnResultReturnedToActor();
    //    //if(currentState.name == "Slay Success") {
    //    //    (poiTarget as Character).Death(deathFromAction: this, responsibleCharacter: actor);
    //    //}
    //}
    #endregion

    #region State Effects
    public void AfterSlaySuccess(ActualGoapNode goapNode) {
        (goapNode.poiTarget as Character).Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
    }
    #endregion

    #region Preconditions
    private bool TargetCannotMove(Character actor, IPointOfInterest target, object[] otherData) {
        return (target as Character).canMove == false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget;
        }
        return false;
    }
    #endregion

    #region Intel Reactions
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
    //            else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
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
    //            else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
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
    //                        recipient.combatComponent.AddAvoidInRange(actor);
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
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else {
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor);
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
    //            else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
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
    //            else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
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
    //                        recipient.combatComponent.AddAvoidInRange(actor);
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
    //                RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
    //                if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor);
    //                    }
    //                    reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
    //                } else {
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor);
    //                    }
    //                    reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    #endregion
}

public class SlayCharacterData : GoapActionData {
    public SlayCharacterData() : base(INTERACTION_TYPE.SLAY_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && (poiTarget as Character).traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER);
    }
}