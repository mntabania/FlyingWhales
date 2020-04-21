namespace Tutorial {
    public abstract class TutorialQuestStep {

        public string stepDescription { get; }
        public string tooltip { get; private set; }
        public bool isCompleted { get; private set; }
        public System.Action onCompleteAction { get; private set; }
        
        protected TutorialQuestStep(string stepDescription, string tooltip) {
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

        #region Completion
        protected void Complete() {
            if (isCompleted) { return; }
            isCompleted = true;
            Messenger.Broadcast(Signals.TUTORIAL_STEP_COMPLETED, this);
            onCompleteAction?.Invoke();
        }
        public TutorialQuestStep SetCompleteAction(System.Action onCompleteAction) {
            this.onCompleteAction = onCompleteAction;
            return this;
        }
        #endregion

        public void Cleanup() {
            UnSubscribeListeners();
        }
    }
}