namespace Tutorial {
    public abstract class TutorialQuestStep {

        public string stepDescription { get; }
        public string tooltip { get; private set; }
        public bool isCompleted { get; private set; }

        public TutorialQuestStep(string stepDescription, string tooltip) {
            this.stepDescription = stepDescription;
            this.tooltip = tooltip;
            isCompleted = false;
        }

        #region Initialization
        /// <summary>
        /// Activate this quest step. This means that this step will start listening for its completion.
        /// </summary>
        public void Activate() {
            SubscribeListeners();
        }
        #endregion

        #region Listeners
        protected abstract void SubscribeListeners();
        protected abstract void UnSubscribeListeners();
        #endregion
        
        public void Complete() {
            if (isCompleted) { return; }
            isCompleted = true;
            Messenger.Broadcast(Signals.TUTORIAL_STEP_COMPLETED, this);
        }

        public void Cleanup() {
            UnSubscribeListeners();
        }
    }
}