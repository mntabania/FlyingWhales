﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Plagued : Trait {

        public Character owner { get; private set; } //poi that has the poison

        private float pukeChance;
        private float septicChance;

        public Plagued() {
            name = "Plagued";
            description = "This character has a terrible disease.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.ticksPerDay * 3;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            mutuallyExclusive = new string[] { "Robust" };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
        }
        protected override void OnChangeLevel() {
            if (level == 1) {
                pukeChance = 5f;
                septicChance = 0.5f;
            } else if (level == 2) {
                pukeChance = 7f;
                septicChance = 1f;
            } else {
                pukeChance = 9f;
                septicChance = 1.5f;
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)
                    && !IsResponsibleForTrait(characterThatWillDoJob)) {
                    if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(characterThatWillDoJob, targetCharacter)) {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob);
                        job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                        job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.HEALING_POTION].craftCost });
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    } else {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob.currentRegion.area);
                        job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                        job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.HEALING_POTION].craftCost });
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
                        characterThatWillDoJob.currentRegion.area.AddToAvailableJobs(job);
                    }
                    return true;
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override bool PerTickOwnerMovement() {
            //string summary = owner.name + " is rolling for plagued chances....";
            float pukeRoll = Random.Range(0f, 100f);
            float septicRoll = Random.Range(0f, 100f);
            bool hasCreatedJob = false;
            if (pukeRoll < pukeChance) {
                //do puke action
                if (owner.characterClass.className == "Zombie" || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)) {
                    return hasCreatedJob;
                }
                ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PUKE], owner, owner, null, 0);
                GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                owner.jobQueue.AddJobInQueue(job);
                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
                //owner.jobQueue.AddJobInQueue(job);
                hasCreatedJob = true;
            } else if (septicRoll < septicChance) {
                if (owner.characterClass.className == "Zombie" || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.SEPTIC_SHOCK)) {
                    return hasCreatedJob;
                }
                ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SEPTIC_SHOCK], owner, owner, null, 0);
                GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                owner.jobQueue.AddJobInQueue(job);
                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);
                //owner.jobQueue.AddJobInQueue(job);
                hasCreatedJob = true;
            }
            return hasCreatedJob;
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) {
                IPointOfInterest target;
                IPointOfInterest infector;
                if (TryGetTargetAndInfector(goapNode, out target, out infector)) { //this is necessary so that this function can determine which of the characters is infecting the other
                    int roll = Random.Range(0, 100);
                    int chance = GetInfectChanceForAction(action);
                    if (roll < chance) {
                        //target will be infected with plague
                        if (target.traitContainer.AddTrait(target, "Plagued", infector as Character)) {
                            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            log.AddToFillers(infector, infector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddLogToInvolvedObjects();
                        }
                    }
                }
            }
        }
        #endregion

        private bool TryGetTargetAndInfector(ActualGoapNode goapNode,  out IPointOfInterest target, out IPointOfInterest infector) {
            if (goapNode.actor == owner) {
                target = goapNode.poiTarget;
                infector = goapNode.actor;
            } else {
                target = goapNode.actor;
                infector = goapNode.poiTarget;
            }
            return true;
        }

        private int GetInfectChanceForAction(INTERACTION_TYPE type) {
            switch (type) {
                case INTERACTION_TYPE.CHAT_CHARACTER:
                    return GetChatInfectChance();
                case INTERACTION_TYPE.MAKE_LOVE:
                    return GetMakeLoveInfectChance();
                case INTERACTION_TYPE.CARRY:
                    return GetCarryInfectChance();
                default:
                    return 0;
            }
        }

        public int GetChatInfectChance() {
            if (level == 1) {
                return 25;
            } else if (level == 2) {
                return 35;
            } else {
                return 45;
            }
        }
        public int GetMakeLoveInfectChance() {
            if (level == 1) {
                return 50;
            } else if (level == 2) {
                return 75;
            } else {
                return 100;
            }
        }
        public int GetCarryInfectChance() {
            if (level == 1) {
                return 50;
            } else if (level == 2) {
                return 75;
            } else {
                return 100;
            }
        }
    }

}
