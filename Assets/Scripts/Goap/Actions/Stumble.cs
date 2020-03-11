using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Stumble : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Stumble() : base(INTERACTION_TYPE.STUMBLE) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        
        animationName = "Sleep Ground";
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Stumble Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreStumbleSuccess(ActualGoapNode goapNode) {
        //TODO: currentState.SetIntelReaction(SuccessReactions);
    }
    public void PerTickStumbleSuccess(ActualGoapNode goapNode) {
        int randomHpToLose = UnityEngine.Random.Range(1, 6);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(goapNode.actor.maxHP * percentMaxHPToLose);
        Debug.Log(
            $"Stumble of {goapNode.actor.name} percent: {percentMaxHPToLose}, max hp: {goapNode.actor.maxHP}, lost hp: {actualHPToLose}");
        goapNode.actor.AdjustHP(-actualHPToLose, ELEMENTAL_TYPE.Normal);
    }
    public void AfterStumbleSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.currentHP <= 0) {
            goapNode.actor.Death(deathFromAction: goapNode);
        }
    }
    #endregion

    //#region Intel Reactions
    //private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //        RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //        if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //            if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                CreateLaughAtJob(recipient, actor);
    //            }
    //        } else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //            if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)
    //                || recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //                //If they are lovers, affairs or relatives and they saw the other: -stumbled
    //                //They will trigger a personal https://trello.com/c/iDsfwQ7d/2845-character-feeling-concerned job
    //                CreateFeelingConcernedJob(recipient, actor);
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                //50% they will trigger a personal https://trello.com/c/iDsfwQ7d/2845-character-feeling-concerned job
    //                if (Random.Range(0, 100) < 50) {
    //                    CreateFeelingConcernedJob(recipient, actor);
    //                }
    //                //50 % they will trigger a personal https://trello.com/c/Gz12n7Af/2847-character-tease job
    //                else {
    //                    CreateTeaseJob(recipient, actor);
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion

    //#region Check Up
    //private bool CreateLaughAtJob(Character characterThatWillDoJob, Character target) {
    //    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT)) {
    //        GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT, target);
    //        characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
    //        return true;
    //    }
    //    return false;
    //}
    //private bool CreateFeelingConcernedJob(Character characterThatWillDoJob, Character target) {
    //    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED)) {
    //        GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED, target);
    //        characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
    //        return true;
    //    }
    //    return false;
    //}
    //private bool CreateTeaseJob(Character characterThatWillDoJob, Character target) {
    //    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.TEASE)) {
    //        GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.TEASE, target);
    //        characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
    //        return true;
    //    }
    //    return false;
    //}
    //#endregion
}

public class StumbleData : GoapActionData {
    public StumbleData() : base(INTERACTION_TYPE.STUMBLE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}