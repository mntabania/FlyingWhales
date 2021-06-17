using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Traits {
    public class Quarantined : Status, CharacterEventDispatcher.ITraitListener, CharacterEventDispatcher.ICarryListener, 
        CharacterEventDispatcher.ILocationListener {
        public override bool isSingleton => true;
        
        public Quarantined() {
            name = "Quarantined";
            description = "Not allowed to move around.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(96);
            hindersMovement = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                character.eventDispatcher.SubscribeToCharacterGainedTrait(this);
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
                character.eventDispatcher.SubscribeToCharacterCarried(this);
                character.eventDispatcher.SubscribeToCharacterLeftStructure(this);
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.eventDispatcher.SubscribeToCharacterGainedTrait(this);
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
                character.eventDispatcher.SubscribeToCharacterCarried(this);
                character.eventDispatcher.SubscribeToCharacterLeftStructure(this);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.eventDispatcher.UnsubscribeToCharacterGainedTrait(this);
                character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
                character.eventDispatcher.UnsubscribeToCharacterCarried(this);
                character.eventDispatcher.UnsubscribeToCharacterLeftStructure(this);
            }
        }
        public override bool OnDeath(Character p_character) {
            p_character.traitContainer.RemoveTrait(p_character, this);
            return base.OnDeath(p_character);
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            if (traitable is Character owner) {
                CheckNeeds(owner);
            }
        }
        public override void OnHourStarted(ITraitable traitable) {
            base.OnHourStarted(traitable);
            if (traitable.gridTileLocation != null && traitable.gridTileLocation.structure is Hospice && traitable.traitContainer.HasTrait("Plagued")) {
                Plagued plagued = traitable.traitContainer.GetTraitOrStatus<Plagued>("Plagued");
                GameDate originalRemovalDate = traitable.traitContainer.GetLatestExpiryDate(plagued.name);
                if (originalRemovalDate.hasValue) {
                    int ticksRemaining = GameManager.Instance.Today().GetTickDifference(originalRemovalDate);
                    if (ticksRemaining > GameManager.ticksPerHour) {
                        GameDate newExpiryDate = originalRemovalDate;
                        newExpiryDate.ReduceTicks(GameManager.ticksPerHour);
                        if (newExpiryDate.IsBefore(GameManager.Instance.Today())) {
                            //if new expiry date has been set to a tick before this tick, then force it to end on the next tick instead. 
                            newExpiryDate = GameManager.Instance.Today();
                            newExpiryDate.AddTicks(1);
                        }
#if DEBUG_LOG
                        Debug.Log($"{traitable.name} Will reschedule Plagued removal to {newExpiryDate.ToString()} from {originalRemovalDate.ToString()}");
#endif
                        traitable.traitContainer.RescheduleLatestTraitRemoval(traitable, plagued, newExpiryDate);    
                    }
                }
            }
        }
        public override string GetTestingData(ITraitable traitable = null) {
            if (traitable != null) {
                if (traitable.traitContainer.scheduleTickets.ContainsKey(this.name)) {
                    TraitRemoveSchedule traitRemoveSchedule = traitable.traitContainer.scheduleTickets[this.name].Last();
                    return $"Lasts until {traitRemoveSchedule.removeDate.ToString()}";
                } else {
                    return $"Lasts until Indefinitely";
                }
            }
            return base.GetTestingData(traitable);
        }
        private void CheckNeeds(Character owner) {
            if(!owner.marker) {
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
                if (!PlanTirednessRecovery(owner)) {
                    PlanHappinessRecovery(owner);
                }
            }
        }
        
#region Tiredness Recovery
        private bool PlanTirednessRecovery(Character owner) {
            if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargetingThis(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                return CreateSleepJob(owner);
            }
            return false;
        }
        private bool CreateSleepJob(Character owner) {
            if (owner.homeStructure != null) {
                if (owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is BedClinic bed) {
                    CreateActualSleepJob(owner, bed);
                    return true;
                }
            }
            return false;
        }
        private void CreateActualSleepJob(Character owner, BedClinic bed) {
            JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
            if (owner.needsComponent.isExhausted) {
                jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
            }
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, owner);
            owner.jobQueue.AddJobInQueue(job);
        }
#endregion
        
#region Happiness Recovery
        private bool PlanHappinessRecovery(Character owner) {
            if ((owner.needsComponent.isSulking || owner.needsComponent.isBored) && !owner.HasJobTargetingThis(JOB_TYPE.HAPPINESS_RECOVERY)) {
                return CreateDaydreamOrPrayJob(owner);
            }
            return false;
        }
        private bool CreateDaydreamOrPrayJob(Character owner) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                CreateActualHappinessRecoveryJob(owner, INTERACTION_TYPE.PRAY);
                return true;
            } else {
                CreateActualHappinessRecoveryJob(owner, INTERACTION_TYPE.DAYDREAM);
                return true;
            }
        }
        private void CreateActualHappinessRecoveryJob(Character owner, INTERACTION_TYPE actionType) {
            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = owner.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
            }
            if (!triggerBrokenhearted) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, actionType, owner, owner);
                job.SetDoNotRecalculate(true);
                owner.jobQueue.AddJobInQueue(job);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
        }
#endregion


#region Listeners
        public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            if (p_gainedTrait is Transforming) {
                //remove quarantined from character whenever it starts transforming into something else
                p_character.traitContainer.RemoveTrait(p_character, this);
            }
        }
        public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            if (p_lostTrait is Plagued) {
                //remove quarantined from character whenever it loses plagued
                p_character.traitContainer.RemoveTrait(p_character, this);
            }
        }
        public void OnCharacterCarried(Character p_character, Character p_carriedBy) {
            if (p_character.tileObjectLocation is BedClinic) {
                p_character.tileObjectLocation?.RemoveUser(p_character); //whenever this character has been carried, then remove it from it's current bed, if it is in one.    
            }
        }
        public void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure) {
            if (p_leftStructure != null && p_leftStructure.structureType == STRUCTURE_TYPE.HOSPICE) {
                //if character left an apothecary then it is no longer quarantined
                p_character.traitContainer.RemoveTrait(p_character, this);
            }
        }
        public void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_leftStructure) { }
        public void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement) { }
        #endregion
    }
}

