using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
namespace Traits {
    public class Catatonic : Status {

        public Character owner { get; private set; }
        
        private float _chanceToRemove;
        private const int MaxDays = 4;

        #region getters
        public float chanceToRemove => _chanceToRemove;
        public override Type serializedData => typeof(SaveDataCatatonic);
        #endregion
        
        public Catatonic() {
            name = "Catatonic";
            description = "In an unresponsive stupor.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
            hindersSocials = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataCatatonic saveDataCatatonic = saveDataTrait as SaveDataCatatonic;
            Assert.IsNotNull(saveDataCatatonic);
            _chanceToRemove = saveDataCatatonic.chanceToRemove;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                Messenger.AddListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                //owner.AdjustMoodValue(-15, this);
                // owner.needsComponent.AdjustDoNotGetBored(1);
                Messenger.AddListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                // owner.needsComponent.AdjustDoNotGetBored(-1);
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            if (traitable is Character owner) {
                CheckTrait(owner);
            }

            //CheckForChaosOrb();
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Catatonic status) {
                _chanceToRemove = status.chanceToRemove;
            }
        }
        #endregion

        private void CheckTrait(Character owner) {
            if (!owner.CanPlanGoap()) {
                return;
            }
            if (!owner.carryComponent.IsNotBeingCarried()) {
                return;
            }
            if (owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
                return;
            }
            if (owner.jobQueue.jobsInQueue.Count > 0) {
                JobQueueItem job = owner.jobQueue.jobsInQueue[0];
                if (job != null) {
                    owner.PerformJob(job);
                }
            } else {
                PlanTirednessRecovery();
            }
        }

        #region Carry/Drop
        private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status) {
            if (p_type == INTERACTION_TYPE.DROP && p_target == owner) {
                if (owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.tileObjectComponent.objHere as Bed);
                }
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
                job.AddOtherData(INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                owner.jobQueue.AddJobInQueue(job);
            } else {
               spooked.TriggerFeelingSpooked();
            }
        }
        #endregion

        #region Removal
        private void CheckRemovalChance() {
            _chanceToRemove = chanceToRemove + GetChanceIncreasePerHour();
            float roll = Random.Range(0f, 100f);
#if DEBUG_LOG
            Debug.Log(
                $"{GameManager.Instance.TodayLogString()} {owner.name} is rolling for chance to remove catatonic. Roll is {roll.ToString()}. Chance is {chanceToRemove.ToString()}");
#endif
            if (roll <= chanceToRemove) {
                owner.traitContainer.RemoveTrait(owner, this);
            }
        }
        private float GetChanceIncreasePerHour() {
            return 100f / (MaxDays * 24f);
        }
#endregion
    }
}

#region Save Data
public class SaveDataCatatonic : SaveDataTrait {
    public float chanceToRemove;

    public override void Save(Trait trait) {
        base.Save(trait);
        Catatonic catatonic = trait as Catatonic;
        Assert.IsNotNull(catatonic);
        chanceToRemove = catatonic.chanceToRemove;
    }
}
#endregion