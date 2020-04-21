namespace Tutorial {
    public class SelectIntelStep : TutorialQuestStep {
        public SelectIntelStep(string stepDescription = "Select an Intel", string tooltip = "") 
            : base(stepDescription, tooltip) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IIntel>(Signals.ACTIVE_INTEL_SET, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IIntel>(Signals.ACTIVE_INTEL_SET, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(IIntel intel) {
            Complete();
        }
        #endregion
    }
}