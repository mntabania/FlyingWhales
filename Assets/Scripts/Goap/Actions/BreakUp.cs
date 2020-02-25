using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class BreakUp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public BreakUp() : base(INTERACTION_TYPE.BREAK_UP) {
        actionIconString = GoapActionStateDB.Social_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //**Effect 1**: Actor - Remove Lover relationship with Target
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", target = GOAP_EFFECT_TARGET.TARGET });
        //**Effect 2**: Actor - Remove Paramour relationship with Target
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Paramour", target = GOAP_EFFECT_TARGET.ACTOR });
        //**Effect 2**: Target - Add Trait Heartbroken
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Heartbroken", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Break Up Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    #endregion

    #region Effects
    public void PreBreakUpSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.poiTarget as Character;
        if (goapNode.actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER)) {
            //**Effect 1**: Actor - Remove Lover relationship with Character 2
            RelationshipManager.Instance.RemoveRelationshipBetween(goapNode.actor, target, RELATIONSHIP_TYPE.LOVER);
            //if the relationship that was removed is lover, change home to a random unoccupied dwelling,
            //otherwise, no home. Reference: https://trello.com/c/JUSt9bEa/1938-broken-up-characters-should-live-in-separate-house
            goapNode.actor.MigrateHomeStructureTo(null);
            goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, goapNode.actor);
            //if (goapNode.actor.currentRegion.area != null) {
            //    goapNode.actor.currentRegion.area.AssignCharacterToDwellingInArea(goapNode.actor);
            //}
        } else {
            //**Effect 2**: Actor - Remove Paramour relationship with Character 2
            RelationshipManager.Instance.RemoveRelationshipBetween(goapNode.actor, target, RELATIONSHIP_TYPE.AFFAIR);
        }
        //**Effect 3**: Target gains Heartbroken trait
        goapNode.poiTarget.traitContainer.AddTrait(target, "Heartbroken", goapNode.actor);
        //goapNode.action.states[goapNode.currentStateName].SetIntelReaction(BreakupSuccessIntelReaction);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            //    return false;
            //}
            if (target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
                return false;
            }
            if (!actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER) && !actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.AFFAIR)) {
                return false; //**Advertised To**: All characters with Lover or Paramour relationship with the character
            }
            return target.IsInOwnParty();
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> BreakupSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;
    //    bool isRecipientLoverOrParamourOfActor = actor.relationshipContainer.HasRelationshipWith(recipient.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || actor.relationshipContainer.HasRelationshipWith(recipient.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR);
    //    bool isRecipientLoverOrParamourOfTarget = targetCharacter.relationshipContainer.HasRelationshipWith(recipient.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || targetCharacter.relationshipContainer.HasRelationshipWith(recipient.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR);

    //    bool isRecipientInLoveWithActor = recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER);
    //    bool isRecipientInLoveWithTarget = recipient.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER);

    //    RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);

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
    //                reactions.Add("I had to do it.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                reactions.Add("Yes, we're done. You don't need to remind me.");
    //            }
    //            //- Recipient is Lover or Paramour of Actor
    //            else if (isRecipientLoverOrParamourOfActor) {
    //                recipient.traitContainer.AddTrait(recipient, "Satisfied");
    //                reactions.Add(string.Format("This is great news! My relationship with {0} will be better now.", actor.name));
    //            }
    //            //- Recipient is Lover or Paramour of Target
    //            else if (isRecipientLoverOrParamourOfActor) {
    //                recipient.traitContainer.AddTrait(recipient, "Satisfied");
    //                reactions.Add(string.Format("This is great news! My relationship with {0} will be better now.", targetCharacter.name));
    //            }
    //            //- Is in Love with Actor
    //            else if (isRecipientInLoveWithActor) {
    //                recipient.traitContainer.AddTrait(recipient, "Satisfied");
    //                reactions.Add(string.Format("This means I have a chance with {0} now.", actor.name));
    //            }
    //            //- Is in Love with Target
    //            else if (isRecipientInLoveWithTarget) {
    //                recipient.traitContainer.AddTrait(recipient, "Satisfied");
    //                reactions.Add(string.Format("This means I have a chance with {0} now.", targetCharacter.name));
    //            }
    //            //- Other Positive Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                reactions.Add(string.Format("Break up sucks! I hope {0} feel better soon.", actor.name));
    //            }
    //            //- Other Positive Relationship with Target
    //            else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                reactions.Add(string.Format("Break up sucks! I hope {0} feel better soon.", targetCharacter.name));
    //            }
    //            //- Negative Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                reactions.Add(string.Format("{0} is a terrible person and does not deserve to be happy.", actor.name));
    //            }
    //            //- Negative Relationship with Target
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                reactions.Add(string.Format("{0} is a terrible person and does not deserve to be happy.", targetCharacter.name));
    //            }
    //            //- Others
    //            else {
    //                reactions.Add("How is this relevant to me?");
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class BreakUpData : GoapActionData {
    public BreakUpData() : base(INTERACTION_TYPE.BREAK_UP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        Character target = poiTarget as Character;
        if (target == actor) {
            return false;
        }
        //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
        //    return false;
        //}
        if (target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        if (!actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER) && !actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.AFFAIR)) {
            return false; //**Advertised To**: All characters with Lover or Paramour relationship with the character
        }
        return target.IsInOwnParty();
    }
}
