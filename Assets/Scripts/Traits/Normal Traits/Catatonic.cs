﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Catatonic : Trait {

        public Character owner { get; private set; }
        public List<Character> charactersThatKnow { get; private set; }

        public Catatonic() {
            name = "Catatonic";
            description = "This character is catatonic.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            daysDuration = GameManager.ticksPerDay;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            charactersThatKnow = new List<Character>();
            hindersMovement = true;
            hindersWitness = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
                owner.AdjustMoodValue(-15, this);
                owner.AdjustDoNotGetLonely(1);
                Messenger.AddListener(Signals.TICK_STARTED, CheckTrait);
                Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (owner != null) {
                owner.AdjustDoNotGetLonely(1);
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckTrait);
                Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead) {
                    if (targetCharacter.faction != characterThatWillDoJob.faction) {
                        if (targetCharacter.traitContainer.GetNormalTrait("Restrained") == null) {
                            GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                            if (currentJob == null) {
                                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.IMPRISON_CHARACTER, targetCharacter);
                                //job.SetCanBeDoneInLocation(true);
                                if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, job)) {
                                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                                    return true;
                                }
                                //else {
                                //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainJob);
                                //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                                //    return false;
                                //}
                            } else {
                                if ((currentJob.currentOwner.ownerType == JOB_QUEUE_OWNER.LOCATION || currentJob.currentOwner.ownerType == JOB_QUEUE_OWNER.QUEST) 
                                    && InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                                    bool canBeTransfered = false;
                                    Character assignedCharacter = currentJob.currentOwner as Character;
                                    if (assignedCharacter != null && assignedCharacter.currentActionNode.action != null
                                        && assignedCharacter.currentJob != null && assignedCharacter.currentJob == currentJob) {
                                        if (assignedCharacter != characterThatWillDoJob) {
                                            canBeTransfered = !assignedCharacter.marker.inVisionPOIs.Contains(assignedCharacter.currentActionNode.poiTarget);
                                        }
                                    } else {
                                        canBeTransfered = true;
                                    }
                                    if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                                        (currentJob.currentOwner as Character).jobQueue.CancelJob(currentJob, shouldDoAfterEffect: false, forceRemove: true);
                                        characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob, false);
                                        //TODO: characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(currentJob, characterThatWillDoJob);
                                        return true;
                                    }
                                }
                            }
                        }
                    } else {
                        if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE) {
                            if (owner.CanPlanGoap() && !owner.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                                if (!PlanFullnessRecovery(characterThatWillDoJob)) {
                                    CreateDropJobForTirednessRecovery(characterThatWillDoJob);
                                }
                            }
                        }
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion

        public void AddCharacterThatKnows(Character character) {
            if (!charactersThatKnow.Contains(character)) {
                charactersThatKnow.Add(character);
            }
        }

        private void CheckTrait() {
            if (!owner.CanPlanGoap()) {
                return;
            }
            if (!owner.IsInOwnParty()) {
                return;
            }
            if (owner.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                return;
            }
            if (owner.allGoapPlans.Count > 0) {
                owner.PerformGoapPlans();
            } else {
                PlanTirednessRecovery();
            }
        }

        #region Carry/Drop
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, owner,
                            new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.DROP, new object[] { dropLocationStructure }
                } });
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, owner,
                           new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile }
                } });
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
            if (action.goapType == INTERACTION_TYPE.DROP && character.currentActionNode.poiTarget == owner) {
                if (owner.gridTileLocation.objHere != null && owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.objHere as Bed);
                }
            }
        }
        #endregion

        #region Fullness Recovery
        private bool PlanFullnessRecovery(Character characterThatWillDoJob) {
            if (owner.isStarving || owner.isHungry) {
                return CreateFeedJob(characterThatWillDoJob);
            }
            return false;
        }
        private bool CreateFeedJob(Character characterThatWillDoJob) {
            if (characterThatWillDoJob.specificLocation.region.IsResident(owner)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = "", isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FEED, owner, new Precondition(goapEffect, null));
                //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeParalyzedFeedJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                return true;
            }
            return false;
        }
        #endregion

        #region Tiredness Recovery
        private bool CreateDropJobForTirednessRecovery(Character characterThatWillDoJob) {
            if (owner.isExhausted || owner.isTired) {
                if (owner.homeStructure != null && (owner.gridTileLocation.objHere == null || !(owner.gridTileLocation.objHere is Bed))) {
                    TileObject bed = owner.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                    if (bed != null) {
                        return CreateActualDropJob(characterThatWillDoJob, owner.homeStructure, bed.gridTileLocation);
                    }
                }
            }
            return false;
        }
        private bool PlanTirednessRecovery() {
            if ((owner.isExhausted || owner.isTired) && !owner.HasJobTargettingThis(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                return CreateSleepJob();
            }
            return false;
        }
        private bool CreateSleepJob() {
            if (owner.homeStructure != null) {
                if (owner.gridTileLocation.objHere != null && owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.objHere as Bed);
                    return true;
                }
                //else {
                //    TileObject bed = character.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                //    if(bed != null){
                //        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure, bed.gridTileLocation);
                //    }
                //}
            }
            return false;
        }
        private void CreateActualSleepJob(Bed bed) {
            JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
            if (owner.isExhausted) {
                jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
            }
            bool triggerSpooked = false;
            Spooked spooked = owner.traitContainer.GetNormalTrait("Spooked") as Spooked;
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerSpooked) {
                GoapPlanJob job = new GoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE } } });
                owner.jobQueue.AddJobInQueue(job);
                //TODO: owner.jobQueue.AssignCharacterToJob(job, owner);
            } else {
                //TODO: owner.jobQueue.AssignCharacterToJob(spooked.TriggerFeelingSpooked(), owner);
            }
        }
        #endregion
    }

    public class SaveDataCatatonic : SaveDataTrait {
        public List<int> charactersThatKnow;

        public override void Save(Trait trait) {
            base.Save(trait);
            Catatonic catatonic = trait as Catatonic;
            charactersThatKnow = new List<int>();
            for (int i = 0; i < catatonic.charactersThatKnow.Count; i++) {
                charactersThatKnow.Add(catatonic.charactersThatKnow[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Catatonic catatonic = trait as Catatonic;
            for (int i = 0; i < charactersThatKnow.Count; i++) {
                catatonic.AddCharacterThatKnows(CharacterManager.Instance.GetCharacterByID(charactersThatKnow[i]));
            }
            return trait;
        }
    }
}
