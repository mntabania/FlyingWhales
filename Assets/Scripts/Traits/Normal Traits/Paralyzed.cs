using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Paralyzed : Status {

        public Character owner { get; private set; }

        public Paralyzed() {
            name = "Paralyzed";
            description = "Permanently unable to move.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            ticksDuration = 0;
            hindersMovement = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character) {
                owner = addTo as Character;
                Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
                if (GameUtilities.RollChance(15) && character.homeSettlement != null && //15 
                    Locations.Settlements.Settlement_Events.PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(character.homeSettlement) && 
                    !character.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && character.homeSettlement.eventManager.CanHaveEvents()) {
                    character.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (owner != null) {
                Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            CheckParalyzedTrait();
        }
        #endregion
        
        private void CheckParalyzedTrait() {
            if(!owner.marker) {
                return;
            }
            if (owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
                return;
            }
            if (owner.jobQueue.jobsInQueue.Count > 0) {
                if (!owner.CanPerformEndTickJobs()) {
                    return;
                }
                JobQueueItem job = owner.jobQueue.jobsInQueue[0];
                if (job != null) {
                    owner.PerformJob(job);
                }
            } else {
                if (!PlanTirednessRecovery()) {
                    PlanHappinessRecovery();
                }
            }
        }

        #region Carry/Drop
        private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status) {
            if (p_type == INTERACTION_TYPE.DROP && p_target == this.owner) {
                if (this.owner.gridTileLocation.tileObjectComponent.objHere != null && this.owner.gridTileLocation.tileObjectComponent.objHere is Bed) {
                    CreateActualSleepJob(this.owner.gridTileLocation.tileObjectComponent.objHere as Bed);
                } else if (this.owner.gridTileLocation.structure == this.owner.homeStructure) {
                    CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                } else {
                    CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                }
            }
        }
        #endregion

        #region Happiness Recovery
        private bool PlanHappinessRecovery() {
            if ((owner.needsComponent.isSulking || owner.needsComponent.isBored) && !owner.HasJobTargetingThis(JOB_TYPE.HAPPINESS_RECOVERY)) {
                return CreateDaydreamOrPrayJob();
            }
            return false;
        }
        private bool CreateDaydreamOrPrayJob() {
            if (owner.currentRegion.IsResident(owner)) {
                if (owner.homeStructure != null && owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
                    if (owner.currentStructure == owner.homeStructure) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                        return true;
                    } else if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                        return true;
                    }
                } else {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                        return true;
                    } else {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CreatePrayJob() {
            if (owner.homeStructure == null || owner.currentStructure == owner.homeStructure) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                return true;
            }
            //else {
            //    if (character.homeStructure != null) {
            //        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            //        return true;
            //    }
            //}
            return false;
        }
        private bool CreateDaydreamJob() {
            if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || !owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                return true;
            }
            //else {
            //    LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
            //    if (structure != null) {
            //        return CreateActualDropJob(characterThatWillDoJob, structure);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            //        return true;
            //    }
            //}
            return false;
        }
        private void CreateActualHappinessRecoveryJob(INTERACTION_TYPE actionType) {
            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = owner.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
            }
            if (!triggerBrokenhearted) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, actionType, owner, owner);
                job.SetDoNotRecalculate(true);
                //job.AddOtherData(actionType, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                owner.jobQueue.AddJobInQueue(job);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
        }
        #endregion

        #region Tiredness Recovery
        private bool PlanTirednessRecovery() {
            if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargetingThis(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                return CreateSleepJob();
            }
            return false;
        }
        private bool CreateSleepJob() {
            if (owner.homeStructure != null) {
                if (owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.tileObjectComponent.objHere as Bed);
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
            JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
            if (owner.needsComponent.isExhausted) {
                jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
            }
            bool triggerSpooked = false;
            Spooked spooked = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[spooked.name]);
            }
            if (!triggerSpooked) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, owner);
                //job.AddOtherData(INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                owner.jobQueue.AddJobInQueue(job);
            } else {
                spooked.TriggerFeelingSpooked();
            }
        }
        #endregion
    }
}
