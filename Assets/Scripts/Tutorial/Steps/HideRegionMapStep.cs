namespace Tutorial {
    public class HideRegionMapStep : TutorialQuestStep {
        public HideRegionMapStep(string stepDescription = "Hide regional map", string tooltip = "") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_CLOSED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(Region region) {
            Complete();
        }
        #endregion
    }
}