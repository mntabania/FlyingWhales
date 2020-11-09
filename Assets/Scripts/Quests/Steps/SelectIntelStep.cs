namespace Quests.Steps {
    public class SelectIntelStep : QuestStep {
        public SelectIntelStep(string stepDescription = "Select an Intel") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IIntel>(PlayerSignals.ACTIVE_INTEL_SET, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IIntel>(PlayerSignals.ACTIVE_INTEL_SET, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(IIntel intel) {
            Complete();
        }
        #endregion
    }
}