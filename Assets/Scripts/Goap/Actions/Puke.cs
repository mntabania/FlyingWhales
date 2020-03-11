using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Puke : GoapAction {

    public Puke() : base(INTERACTION_TYPE.PUKE) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Puke Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 5;
    }
    #endregion

    #region State Effects
    public void PrePukeSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.INACTIVE);
        //TODO: currentState.SetIntelReaction(SuccessReactions);
    }
    public void AfterPukeSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.ACTIVE);
        //if (recipient != null) {
        //    CreateRemoveTraitJob(recipient);
        //}
        //isPuking = false;
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
    //private void CreateRemoveTraitJob(Character characterThatWillDoJob) {
    //    Trait trait = actor.traitContainer.GetNormalTrait<Trait>("Plagued", "Infected", "Sick");
    //    if (trait != null && !actor.isDead && !actor.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, trait.name) && !actor.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
    //        SerialKiller serialKiller = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Psychopath") as SerialKiller;
    //        if (serialKiller != null) {
    //            serialKiller.SerialKillerSawButWillNotAssist(actor, trait);
    //            return;
    //        }
    //        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, targetPOI = actor };
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
    //            new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
    //        if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(characterThatWillDoJob, actor, job)) {
    //            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
    //        } else {
    //            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
    //            characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
    //        }
    //    }
    //}

    //#region Intel Reactions
    //private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //    //- Is Actor
    //    if (recipient == actor) {
    //        //  - If Informed: "That was embarrassing."
    //        if (status == SHARE_INTEL_STATUS.INFORMED) {
    //            reactions.Add("That was embarrassing.");
    //        }
    //    }
    //    //- Neutral or Positive Relationship with Actor
    //    else if (relWithActor == RELATIONSHIP_EFFECT.NONE || relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //        if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //            //- If witnessed: 35% chance to also Puke if also on the move
    //            if (Random.Range(0, 100) < 35 && recipient.marker.isMoving) {
    //                if (AlsoPuke(recipient)) {
    //                    return reactions; //do not do anything else
    //                }
    //            }
    //        }
    //        if (!isPuking) {
    //            Trait trait = actor.traitContainer.GetNormalTrait<Trait>("Plagued", "Infected", "Sick");
    //            if (trait != null && !actor.isDead && !actor.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, trait.name) && !actor.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
    //                SerialKiller serialKiller = recipient.traitContainer.GetNormalTrait<Trait>("Psychopath") as SerialKiller;
    //                if (serialKiller != null) {
    //                    serialKiller.SerialKillerSawButWillNotAssist(actor, trait);
    //                } else {
    //                    GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, targetPOI = actor };
    //                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
    //                        new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
    //                    if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(recipient, actor, job)) {
    //                        if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                            //- if informed: "I'm a doctor. I should help [Actor Name]."
    //                            reactions.Add(string.Format("I'm a doctor. I should help {0}.", recipient.name));
    //                        }
    //                        recipient.jobQueue.AddJobInQueue(job);
    //                    } else {
    //                        if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                            reactions.Add(string.Format("I hope {0} gets well soon.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                        }
    //                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
    //                        recipient.specificLocation.jobQueue.AddJobInQueue(job);
    //                    }
    //                }
    //            }
    //        } else {
    //            //If currently puking, wait until puking is done, then process remove trait job on AfterPukeSuccess
    //            //This is done because character is inactive while puking, it will only result in "unable to do action" if we create a job while puking, so we need to wait until it is finished
    //            if (this.recipient == null) {
    //                this.recipient = recipient;
    //            }
    //            if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                if(recipient.traitContainer.GetNormalTrait<Trait>("Healer") != null) {
    //                    //- if informed: "I'm a doctor. I should help [Actor Name]."
    //                    reactions.Add(string.Format("I'm a doctor. I should help {0}.", recipient.name));
    //                } else {
    //                    reactions.Add(string.Format("I hope {0} gets well soon.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                }
    //            }

    //            #region Check Up
    //            else if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE && !recipient.jobQueue.HasJob(JOB_TYPE.REMOVE_TRAIT, actor)) {
    //                    CreateFeelingConcernedJob(recipient, actor);
    //                }
    //            }
    //            #endregion

    //        }
    //    }
    //    //- Negative Relationship with Actor
    //    else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //        if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //            //- If witnessed: 35% chance to also Puke if also on the move
    //            if (Random.Range(0, 100) < 35 && recipient.marker.isMoving) {
    //                if (AlsoPuke(recipient)) {
    //                    return reactions; //do not do anything else
    //                }                    
    //            }
    //            #region Check Up
    //            if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                CreateLaughAtJob(recipient, actor);
    //            }
    //            #endregion
    //        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //            //- If Informed: "Stop sharing gross things about that vile person."
    //            reactions.Add("Stop sharing gross things about that vile person.");
    //        }
    //    }

    //    return reactions;
    //}
    //#endregion

    //private GoapAction stoppedAction;
    //private CharacterState pausedState;
    //private bool AlsoPuke(Character character) {
    //    if (character.currentActionNode != null && character.currentActionNode.action.goapType != INTERACTION_TYPE.PUKE) {
    //        stoppedAction = character.currentActionNode;
    //        character.StopCurrentAction(false);
    //        character.marker.StopMovement();

    //        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, character, character);

    //        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
    //        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
    //        job.SetAssignedPlan(goapPlan);
    //        goapPlan.ConstructAllNodes();
    //        goapAction.CreateStates();
    //        character.jobQueue.AddJobInQueue(job, false);

    //        goapAction.SetEndAction(ResumeLastAction);
    //        goapAction.DoAction();
    //        return true;
    //    } else if (character.stateComponent.currentState != null) {
    //        pausedState = character.stateComponent.currentState;
    //        character.stateComponent.currentState.PauseState();
    //        character.marker.StopMovement();
    //        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, character, character);

    //        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
    //        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
    //        job.SetAssignedPlan(goapPlan);
    //        goapPlan.ConstructAllNodes();
    //        goapAction.CreateStates();
    //        character.jobQueue.AddJobInQueue(job, false);

    //        goapAction.SetEndAction(ResumePausedState);
    //        goapAction.DoAction();
    //        return true;
    //    } else if (character.stateComponent.currentState == null && character.currentActionNode == null) {
    //        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, character, character);

    //        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
    //        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
    //        job.SetAssignedPlan(goapPlan);
    //        goapPlan.ConstructAllNodes();
    //        goapAction.CreateStates();
    //        character.jobQueue.AddJobInQueue(job, false);

    //        goapAction.DoAction();
    //        return true;
    //    }
    //    return false;
    //}

    //private void ResumeLastAction(string result, GoapAction action) {
    //    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(stoppedAction.goapType, stoppedAction.actor, stoppedAction.poiTarget, stoppedAction.otherData)) {
    //        stoppedAction.DoAction();
    //    } else {
    //        action.actor.GoapActionResult(result, action);
    //    }

    //}
    //private void ResumePausedState(string result, GoapAction action) {
    //    action.actor.GoapActionResult(result, action);
    //    pausedState.ResumeState();
    //}

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
    //#endregion

    //private bool CanCharacterTakeRemoveIllnessesJob(Character character, Character targetCharacter, JobQueueItem job) {
    //    if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeArea) {
    //        if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
    //            return character.race == targetCharacter.race && character.homeNpcSettlement == targetCharacter.homeNpcSettlement && !targetCharacter.HasRelationshipOfTypeWith(character, RELATIONSHIP_TRAIT.ENEMY);
    //        }
    //        return !character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY) && character.traitContainer.GetNormalTrait<Trait>("Healer") != null;
    //    }
    //    return false;
    //}
}

public class PukeData : GoapActionData {
    public PukeData() : base(INTERACTION_TYPE.PUKE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }
}
