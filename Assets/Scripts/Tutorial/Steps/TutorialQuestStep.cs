namespace Tutorial {
    public abstract class TutorialQuestStep {

        public string stepDescription { get; } 
        public bool isCompleted { get; private set; }
        private System.Action onCompleteAction { get; set; }
        public System.Action<TutorialQuestStepItem> onHoverOverAction { get; private set; }
        public System.Action onHoverOutAction { get; private set; }

        #region getters
        public bool hasHoverAction => onHoverOverAction != null || onHoverOutAction != null;
        #endregion

        protected TutorialQuestStep(string stepDescription) {
            this.stepDescription = stepDescription;
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

        #region Hover Actions
        public TutorialQuestStep SetHoverOverAction(System.Action<TutorialQuestStepItem> onHoverOverAction) {
            this.onHoverOverAction = onHoverOverAction;
            return this;
        }
        public TutorialQuestStep SetHoverOutAction(System.Action onHoverOutAction) {
            this.onHoverOutAction = onHoverOutAction;
            return this;
        }
        #endregion
        

        public void Cleanup() {
            UnSubscribeListeners();
        }
    }
}