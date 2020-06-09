using Ruinarch.Custom_UI;
namespace Quests.Steps {
    public class ToggleTurnedOnStep : QuestStep {
        
        private readonly string _neededIdentifier;
        
        public ToggleTurnedOnStep(string neededIdentifier, string stepDescription) 
            : base(stepDescription) {
            _neededIdentifier = neededIdentifier;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<RuinarchToggle>(Signals.TOGGLE_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<RuinarchToggle>(Signals.TOGGLE_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(RuinarchToggle toggle) {
            if (toggle.isOn && toggle.name == _neededIdentifier) {
                Complete();
            }
        }
        #endregion
    }
}