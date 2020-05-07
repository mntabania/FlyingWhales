namespace Quests.Steps {
    public class ToggleTurnedOnStep : QuestStep {
        
        private readonly string _neededIdentifier;
        
        public ToggleTurnedOnStep(string neededIdentifier, string stepDescription) 
            : base(stepDescription) {
            _neededIdentifier = neededIdentifier;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<string>(Signals.TOGGLE_TURNED_ON, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<string>(Signals.TOGGLE_TURNED_ON, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(string identifier) {
            if (identifier == _neededIdentifier) {
                Complete();
            }
        }
        #endregion
    }
}