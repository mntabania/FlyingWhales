using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CurseCharacter : GoapAction {

    public CurseCharacter() : base(INTERACTION_TYPE.CURSE_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        shouldAddLogs = false; //set to false because this action has a special case for logs
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Cursed", target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        List<TileObject> magicCircle = goapNode.actor.currentRegion.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
        TileObject chosen = magicCircle[Random.Range(0, magicCircle.Count)];
        return chosen;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Curse Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 3;
    }
    #endregion

    #region State Effects
    public void PreCurseSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterCurseSuccess(ActualGoapNode goapNode) {
        //**After Effect 1**: Target gains Cursed trait.
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Cursed", goapNode.actor);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && actor.currentRegion.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.MAGIC_CIRCLE) > 0;
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //public List<string> CurseSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
    //                reactions.Add("I know what I did.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                bool hasRelationshipDegraded = false;
    //                if (!hasCrimeBeenReported) {
    //                    hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                }
    //                if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    if (hasRelationshipDegraded) {
    //                        reactions.Add(string.Format("So it was {0} who did that to me. I should start avoiding {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                    } else {
    //                        reactions.Add(string.Format("I forgave {0} already.", actor.name));
    //                    }
    //                } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add(string.Format("So it was {0} who did that to me. I should get back at {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                    if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                    }
    //                } else {
    //                    reactions.Add(string.Format("Why did {0} do that to me?", actor.name));
    //                    AddTraitTo(recipient, "Annoyed");
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                bool hasRelationshipDegraded = false;
    //                if (!hasCrimeBeenReported) {
    //                    hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                }
    //                if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    if (hasRelationshipDegraded) {
    //                        reactions.Add(string.Format("{0} shouldn't have done that to {1}. I should start avoiding {2}.", actor.name, targetCharacter.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                    } else {
    //                        reactions.Add("Everyone makes mistakes.");
    //                    }
    //                } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add(string.Format("Why did {0} do that to {1}? I should get back at {2}.", actor.name, targetCharacter.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                    if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                    }
    //                } else {
    //                    reactions.Add(string.Format("Why did {0} do that to {1}?", actor.name, targetCharacter.name));
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                reactions.Add(string.Format("Serves {0} right.", targetCharacter.name));
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                bool hasRelationshipDegraded = false;
    //                if (!hasCrimeBeenReported) {
    //                    hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                }
    //                if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    if (hasRelationshipDegraded) {
    //                        reactions.Add(string.Format("{0} shouldn't have done that to {1}. I should start avoiding {2}.", actor.name, targetCharacter.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                    } else {
    //                        reactions.Add(string.Format("{0} probably has {1} reason for doing that.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.POSSESSIVE, false)));
    //                    }
    //                } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add(string.Format("{0} is up to no good again.", actor.name));
    //                } else {
    //                    reactions.Add(string.Format("Why did {0} do that to {1}?", actor.name, targetCharacter.name));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class CurseCharacterData : GoapActionData {
    public CurseCharacterData() : base(INTERACTION_TYPE.CURSE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}