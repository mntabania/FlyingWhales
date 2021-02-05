﻿namespace Traits {
    public class Mining : Status {

        private Character _owner;

        public Mining() {
            name = "Mining";
            description = "This is Mining.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(5);
            isHidden = true;
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                _owner = character;
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                //Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
                //Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnMineJobRemoved);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(MineBehaviour));
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                //Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
                //Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnMineJobRemoved);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(MineBehaviour));
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
            }
        }
        #endregion
        
        private void OnJobAddedToQueue(JobQueueItem job, Character character) {
            if (_owner == character && 
                (job.jobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || job.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT
                || job.jobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL || job.jobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT)) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        //private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
        //    if (character == _owner && job.jobType == JOB_TYPE.MINE && job.targetInteractionType == INTERACTION_TYPE.MINE) {
        //        character.behaviourComponent.SetTargetMiningTile(null);
        //    }
        //}
        //private void OnMineJobRemoved(JobQueueItem job, Character character) {
        //    if (character == _owner && job.jobType == JOB_TYPE.MINE) {
        //        character.behaviourComponent.SetTargetMiningTile(null);
        //    }
        //}
    }
}