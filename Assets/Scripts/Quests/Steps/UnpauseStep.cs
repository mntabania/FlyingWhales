namespace Quests.Steps {
    public class UnpauseStep : QuestStep {
        public UnpauseStep(string stepDescription = "Unpause the game") : base(stepDescription) {
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<bool>(Signals.PAUSED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<bool>(Signals.PAUSED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(bool isPaused) {
            if (isPaused == false) {
                Complete();
            }
        }
        #endregion
    }
}