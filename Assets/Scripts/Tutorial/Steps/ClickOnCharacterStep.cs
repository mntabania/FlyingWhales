using System;
namespace Tutorial {
    public class ClickOnCharacterStep : TutorialQuestStep {
        private readonly Func<Character, bool> _validityChecker;
        public ClickOnCharacterStep(string stepDescription = "Click on a character", string tooltip = "", 
            System.Func<Character, bool> validityChecker = null) : base(stepDescription, tooltip) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(ISelectable selectable) {
            if (selectable is Character character) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(character)) {
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