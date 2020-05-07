namespace Quests.Steps {
    public class SelectRegionStep : QuestStep {
        public SelectRegionStep(string stepDescription = "Select a Region") : base(stepDescription) { }
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