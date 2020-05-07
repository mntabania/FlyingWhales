using JetBrains.Annotations;
namespace Quests.Steps {
    public class GoapJobFailed : QuestStep {
        
        private readonly Character _target;
        private readonly GoapPlanJob _job;
        
        public GoapJobFailed(string stepDescription, Character target, [NotNull]GoapPlanJob job) : base(stepDescription) {
            _target = target;
            _job = job;
        }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
        }

        #region Completion
        private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
            if (_target == character && job == _job && _job.finishedSuccessfully == false) {
                Complete();
            }
        }
        #endregion
    }
}