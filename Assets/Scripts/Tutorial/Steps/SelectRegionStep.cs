namespace Tutorial {
    public class SelectRegionStep : TutorialQuestStep {
        public SelectRegionStep(string stepDescription = "Select a Region", string tooltip = "") : base(stepDescription, tooltip) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Region>(Signals.REGION_SELECTED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Region>(Signals.REGION_SELECTED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(Region region) {
            Complete();
        }
        #endregion
    }
}