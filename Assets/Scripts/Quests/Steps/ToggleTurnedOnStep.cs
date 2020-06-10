using System;
using Ruinarch.Custom_UI;
namespace Quests.Steps {
    public class ToggleTurnedOnStep : QuestStep {
        
        private readonly string _neededIdentifier;
        private readonly Func<bool> _isToggleValid;
        
        public ToggleTurnedOnStep(string neededIdentifier, string stepDescription, System.Func<bool> isToggleValid = null) 
            : base(stepDescription) {
            _neededIdentifier = neededIdentifier;
            _isToggleValid = isToggleValid;
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
                if (_isToggleValid != null) {
                    if (_isToggleValid.Invoke()) {
                        Complete();
                    }
                } else {
                    Complete();    
                }
            }
        }
        #endregion
    }
}