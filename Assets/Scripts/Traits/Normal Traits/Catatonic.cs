using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Catatonic : Trait {

        public Character owner { get; private set; }

        private float chanceToRemove;
        private const int MaxDays = 4;
        
        public Catatonic() {
            name = "Catatonic";
            description = "This character is catatonic.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
                //owner.AdjustMoodValue(-15, this);
                // owner.needsComponent.AdjustDoNotGetBored(1);
                Messenger.AddListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (owner != null) {
                // owner.needsComponent.AdjustDoNotGetBored(-1);
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            CheckTrait();
            CheckForChaosOrb();
        }
        #endregion

        private void CheckTrait() {
            if (!owner.CanPlanGoap()) {
                return;
            }
            if (!owner.IsInOwnParty()) {
                return;
            }
            if (owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
                return;
            }
            if (owner.jobQueue.jobsInQueue.Count > 0) {
                owner.PerformTopPriorityJob();
            } else {
                PlanTirednessRecovery();
            }
        }

        #region Carry/Drop
        private void OnCharacterFinishedAction(ActualGoapNode node) {
            if (node.action.goapType == INTERACTION_TYPE.DROP && node.poiTarget == owner) {
                if (owner.gridTileLocation.objHere != null && owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.objHere as Bed);
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
            JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
            if (owner.needsComponent.isExhausted) {
                jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
            }
            bool triggerSpooked = false;
            Spooked spooked = owner.traitContainer.GetNormalTrait<Spooked>("Spooked");
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
            chanceToRemove += GetChanceIncreasePerHour();
            float roll = Random.Range(0f, 100f);
            Debug.Log(
                $"{GameManager.Instance.TodayLogString()} {owner.name} is rolling for chance to remove catatonic. Roll is {roll.ToString()}. Chance is {chanceToRemove.ToString()}");
            if (roll <= chanceToRemove) {
                owner.traitContainer.RemoveTrait(owner, this);
            }
        }
        private float GetChanceIncreasePerHour() {
            return 100f / (MaxDays * 24f);
        }
        #endregion

        #region Chaos Orb
        private void CheckForChaosOrb() {
            string summary = $"{owner.name} is rolling for chaos orb in catatonic trait";
            int roll = Random.Range(0, 100);
            int chance = 5;
            summary += $"\nRoll is {roll.ToString()}. Chance is {chance.ToString()}";
            if (roll < chance) {
                Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, owner.marker.transform.position, 
                    1, owner.currentRegion.innerMap);
            }
            owner.logComponent.PrintLogIfActive(summary);
        }
        #endregion
    }

    //public class SaveDataCatatonic : SaveDataTrait {
    //    public List<int> charactersThatKnow;

    //    public override void Save(Trait trait) {
    //        base.Save(trait);
    //        Catatonic catatonic = trait as Catatonic;
    //        charactersThatKnow = new List<int>();
    //        for (int i = 0; i < catatonic.charactersThatKnow.Count; i++) {
    //            charactersThatKnow.Add(catatonic.charactersThatKnow[i].id);
    //        }
    //    }

    //    public override Trait Load(ref Character responsibleCharacter) {
    //        Trait trait = base.Load(ref responsibleCharacter);
    //        Catatonic catatonic = trait as Catatonic;
    //        for (int i = 0; i < charactersThatKnow.Count; i++) {
    //            catatonic.AddCharacterThatKnows(CharacterManager.Instance.GetCharacterByID(charactersThatKnow[i]));
    //        }
    //        return trait;
    //    }
    //}
}
