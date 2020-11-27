using System;
using Ruinarch.Custom_UI;
namespace Quests.Steps {
    public class ButtonClickedStep : QuestStep {
        
        private readonly string _neededIdentifier;
        
        public ButtonClickedStep(string neededIdentifier, string stepDescription) 
            : base(stepDescription) {
            _neededIdentifier = neededIdentifier;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<RuinarchButton>(UISignals.BUTTON_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<RuinarchButton>(UISignals.BUTTON_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(RuinarchButton button) {
            if (button.name == _neededIdentifier) {
                Complete();    
            }
        }
        #endregion
    }
}