namespace Traits {
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

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(MineBehaviour));
                Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(MineBehaviour));
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
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
        private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
            if (character == _owner && job.jobType == JOB_TYPE.MINE && job.targetInteractionType == INTERACTION_TYPE.MINE) {
                character.behaviourComponent.SetTargetMiningTile(null);
            }
        }
    }
}