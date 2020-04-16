using System;
namespace Tutorial {
    public class ToggleTurnedOnStep : TutorialQuestStep {
        
        private readonly string _neededIdentifier;
        
        public ToggleTurnedOnStep(string neededIdentifier, string stepDescription, string tooltip = "") 
            : base(stepDescription, tooltip) {
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