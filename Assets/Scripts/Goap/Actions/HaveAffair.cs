using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class HaveAffair : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public HaveAffair() : base(INTERACTION_TYPE.HAVE_AFFAIR) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Flirt_Icon;
        showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Affair Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        Character otherCharacter = target as Character;
        Character currCharacter = actor;
        List<RELATIONSHIP_TYPE> existingRelsOfCurrentCharacter = currCharacter.relationshipContainer.GetRelationshipDataWith(otherCharacter)?.relationships ?? null;
        List<RELATIONSHIP_TYPE> existingRelsOfOtherCharacter = otherCharacter.relationshipContainer.GetRelationshipDataWith(currCharacter)?.relationships ?? null;
        int cost = 1;
        if (existingRelsOfCurrentCharacter != null) {
            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TYPE.RELATIVE)) {
                //- character is a relative: Weight +50
                cost += 50;
            }
            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TYPE.LOVER)
                || currCharacter.relationshipContainer.IsEnemiesWith(otherCharacter)) {
                //- character is a lover: Weight x0
                //- character is an enemy: Weight x0
                cost *= 0;
            }
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(otherCharacter.race)) {
            //- character is beast 0 out weight
            cost *= 0;
        }
        return cost;
    }
    #endregion

    #region Effects
    public void PreAffairSuccess() {
        //currentState.SetIntelReaction(AffairSuccessReactions);
    }
    public void AfterAffairSuccess(ActualGoapNode goapNode) {
        RelationshipManager.Instance.CreateNewRelationshipBetween(goapNode.actor, goapNode.poiTarget as Character, RELATIONSHIP_TYPE.AFFAIR);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable()) {
                return false;
            }
            if (actor == poiTarget) {
                return false;
            }
            Character targetCharacter = poiTarget as Character;
            SEXUALITY sexuality1 = actor.sexuality;
            SEXUALITY sexuality2 = targetCharacter.sexuality;
            GENDER gender1 = actor.gender;
            GENDER gender2 = targetCharacter.gender;
            if (RelationshipManager.IsSexuallyCompatible(sexuality1, sexuality2, gender1, gender2) 
                && RelationshipManager.Instance.GetValidator(actor).CanHaveRelationship(actor, targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> AffairSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;
    //    //RELATIONSHIP_EFFECT recipientRelationshipWithActor = recipient.GetRelationshipEffectWith(actor);
    //    //RELATIONSHIP_EFFECT recipientRelationshipWithTarget = recipient.GetRelationshipEffectWith(target);
    //    Character actorLover = (actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER) as AlterEgoData)?.owner ?? null;
    //    Character targetLover = (target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER) as AlterEgoData)?.owner ?? null;
    //    Character actorParamour = (actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR) as AlterEgoData)?.owner ?? null;
    //    Character targetParamour = (target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR) as AlterEgoData)?.owner ?? null;


    //    bool hasFled = false;
    //    if (isOldNews) {
    //        reactions.Add("This is old news.");
    //        if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //            hasFled = true;
    //            recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //        }
    //    } else {
    //        //- Recipient is the Actor
    //        if (recipient == actor) {
    //            if (targetLover == recipient) {
    //                reactions.Add("That's private!");
    //            } else if (targetParamour == recipient) {
    //                reactions.Add("Don't tell anyone. *wink**wink*");
    //            }
    //        }
    //        //- Recipient is the Target
    //        else if (recipient == target) {
    //            if (actorLover == recipient) {
    //                reactions.Add("That's private!");
    //            } else if (actorParamour == recipient) {
    //                reactions.Add("Don't you dare judge me!");
    //            }
    //        }
    //        //- Recipient is Actor's Lover
    //        else if (recipient == actorLover) {
    //            string response = string.Empty;
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                response = string.Format("I've had enough of {0}'s shenanigans!", actor.name);
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else {
    //                response = string.Format("I'm still the one {0} comes home to.", actor.name);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                }
    //            }
    //            if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} seduced both of us. {1} must pay for this.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" I already know that {0} is a harlot.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" {0} is my blood. Blood is thicker than water.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" My friendship with {0} is much stronger than this incident.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else if (!recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" I'm not even going to bother myself with {0}.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            }
    //            reactions.Add(response);
    //        }
    //        //- Recipient is Target's Lover
    //        else if (recipient == targetLover) {
    //            string response = string.Empty;
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                response = string.Format("I've had enough of {0}'s shenanigans!", target.name);
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else {
    //                response = string.Format("I'm still the one {0} comes home to.", target.name);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                }
    //            }
    //            if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} seduced both of us. {1} must pay for this.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" I already know that {0} is a harlot.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" {0} is my blood. Blood is thicker than water.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" My friendship with {0} is much stronger than this incident.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" I'm not even going to bother myself with {0}.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            }
    //            reactions.Add(response);
    //        }
    //        //- Recipient is Actor/Target's Paramour
    //        else if (recipient == actorParamour || recipient == targetParamour) {
    //            reactions.Add("I have no right to complain. Bu..but I wish that we could be like that.");
    //            AddTraitTo(recipient, "Heartbroken");
    //        }
    //        //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                recipient.CreateShareInformationJob(actorLover, this);
    //            } else {
    //                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.name));
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                }
    //            }
    //        }
    //        //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", target.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                recipient.CreateShareInformationJob(targetLover, this);
    //            } else {
    //                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.name));
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                }
    //            }
    //        }
    //        //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //            if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                hasFled = true;
    //                recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //            }
    //        }
    //        //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", target.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //            if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                hasFled = true;
    //                recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //            }
    //        }
    //        //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover.currentAlterEgo) == RELATIONSHIP_EFFECT.NONE && actorLover != target) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.name));
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //        }
    //        //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover.currentAlterEgo) == RELATIONSHIP_EFFECT.NONE && targetLover != actor) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.name));
    //            RelationshipManager.Instance.RelationshipDegradation(target, recipient, this);
    //        }
    //        //- Else Catcher
    //        else {
    //            reactions.Add("That is none of my business.");
    //            if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                hasFled = true;
    //                recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //            }
    //        }
    //    }

    //    if (status == SHARE_INTEL_STATUS.WITNESSED && !hasFled) {
    //        if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)
    //            || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            recipient.CreateWatchEvent(this, null, actor);
    //        } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)
    //            || recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            recipient.CreateWatchEvent(this, null, target);
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class HaveAffairData : GoapActionData {
    public HaveAffairData() : base(INTERACTION_TYPE.HAVE_AFFAIR) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        if (actor == poiTarget) {
            return false;
        }
        // Character targetCharacter = poiTarget as Character;
        // if (RelationshipManager.Instance.IsSexuallyCompatible(actor, targetCharacter, TODO, TODO) && RelationshipManager.Instance.GetValidator(actor).CanHaveRelationship(actor, targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
        //     return true;
        // }
        return false;
    }
}