using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ReleaseCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ReleaseCharacter() : base(INTERACTION_TYPE.RELEASE_CHARACTER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Tool", target = GOAP_EFFECT_TARGET.ACTOR }, HasItemTool);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Release Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Preconditions
    private bool HasItemTool(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem(TILE_OBJECT_TYPE.TOOL);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character target = poiTarget as Character;
            return target.traitContainer.HasTrait("Restrained");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreReleaseSuccess(ActualGoapNode goapNode) {
        //TODO: currentState.SetIntelReaction(ReleaseSuccessIntelReaction);
    }
    public void AfterReleaseSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.poiTarget as Character;
        target.traitContainer.RemoveTrait(target, "Restrained");
        target.traitContainer.RemoveTrait(target, "Abducted");
    }
    #endregion

    //#region Intel Reactions
    //private List<string> ReleaseSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo);

    //    //Recipient and Actor are the same
    //    if (recipient == actor) {
    //        //- **Recipient Response Text**: "I know what I've done!"
    //        reactions.Add("I know what I've done!");
    //        //- **Recipient Effect**:  no effect
    //    }
    //    //Recipient considers Target a personal Enemy:
    //    else if (recipient.relationshipContainer.HasRelationshipWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Target Name] should not have been released!"
    //        reactions.Add(string.Format("{0} should not have been released!", target.name));
    //        //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
    //        RelationshipManager.Instance.RelationshipDegradation(actorAlterEgo, recipient);
    //    }
    //    //Recipient considers Actor a personal Enemy:
    //    else if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Actor Name] probably has an ulterior motive for doing that."
    //        reactions.Add(string.Format("{0} probably has an ulterior motive for doing that.", actor.name));
    //        //- **Recipient Effect**:  no effect
    //    }
    //    //Recipient has a positive relationship with the target:
    //    else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //        //- **Recipient Response Text**: "I am relieved that [Target Name] has been released."
    //        reactions.Add(string.Format("I am relieved that {0} has been released.", target.name));
    //        //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
    //        RelationshipManager.Instance.RelationshipDegradation(actorAlterEgo, recipient);
    //    }
    //    //Recipient and Target have no relationship but are from the same faction:
    //    else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
    //        //- **Recipient Response Text**: "I am relieved that [Target Name] has been released."
    //        reactions.Add(string.Format("I am relieved that {0} has been released.", target.name));
    //        //- **Recipient Effect**:  no effect
    //    } 
    //    else {
    //        reactions.Add("This does not concern me.");
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class ReleaseCharacterData : GoapActionData {
    public ReleaseCharacterData() : base(INTERACTION_TYPE.RELEASE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            return target.traitContainer.HasTrait("Restrained");
        }
        return false;
    }
}
