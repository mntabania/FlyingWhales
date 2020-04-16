namespace Tutorial {
    public class UnpauseStep : TutorialQuestStep {
        public UnpauseStep(string stepDescription = "Unpause the game", string tooltip = "") 
            : base(stepDescription, tooltip) {
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