using System;
namespace Tutorial {
    public class ClickOnAreaStep : TutorialQuestStep {
        private readonly Func<HexTile, bool> _validityChecker;
        public ClickOnAreaStep(string stepDescription = "Click on an area", string tooltip = "", 
            Func<HexTile, bool> validityChecker = null) : base(stepDescription, tooltip) {
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
            if (selectable is HexTile hexTile) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(hexTile)) {
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