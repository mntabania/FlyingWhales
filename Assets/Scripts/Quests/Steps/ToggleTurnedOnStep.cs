using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using UnityEngine.Assertions;
namespace Quests.Steps {
    public class ToggleTurnedOnStep : QuestStep {
        
        private readonly string _neededIdentifier;
        private readonly Func<bool> _isToggleValid;
        private readonly List<string> _validIdentifiers;
        
        
        public string neededToggleName => _neededIdentifier;
        
        public ToggleTurnedOnStep(string neededIdentifier, string stepDescription, System.Func<bool> isToggleValid = null) : base(stepDescription) {
            _neededIdentifier = neededIdentifier;
            _isToggleValid = isToggleValid;
        }
        public ToggleTurnedOnStep(List<string> validIdentifiers, string stepDescription, System.Func<bool> isToggleValid = null) : base(stepDescription) {
            _validIdentifiers = validIdentifiers;
            _isToggleValid = isToggleValid;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<RuinarchToggle>(Signals.TOGGLE_CLICKED, CheckForCompletion);
            Messenger.AddListener<RuinarchToggle>(Signals.TOGGLE_SHOWN, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<RuinarchToggle>(Signals.TOGGLE_CLICKED, CheckForCompletion);
            Messenger.RemoveListener<RuinarchToggle>(Signals.TOGGLE_SHOWN, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(RuinarchToggle toggle) {
            if (toggle.isOn) {
                if (DoesToggleMatchIdentifier(toggle)) {
                    if (_isToggleValid != null) {
                        if (_isToggleValid.Invoke()) {
                            Complete();
                        }
                    } else {
                        Complete();    
                    }    
                }
            }
        }
        #endregion

        #region Utilities
        public bool DoesToggleMatchIdentifier(RuinarchToggle toggle) {
            bool doesToggleMatchIdentifier;
            if (!string.IsNullOrEmpty(_neededIdentifier)) {
                doesToggleMatchIdentifier = toggle.name == _neededIdentifier;
            } else {
                Assert.IsNotNull(_validIdentifiers);
                doesToggleMatchIdentifier = _validIdentifiers.Contains(toggle.name);
            }
            return doesToggleMatchIdentifier;
        }
        #endregion
    }
}