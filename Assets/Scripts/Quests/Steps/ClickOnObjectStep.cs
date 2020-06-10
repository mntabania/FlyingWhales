using System;
namespace Quests.Steps {
    public class ClickOnObjectStep : QuestStep {
        private readonly Func<TileObject, bool> _validityChecker;
        public ClickOnObjectStep(string stepDescription = "Click on an object", System.Func<TileObject, bool> validityChecker = null) 
            : base(stepDescription) {
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
            if (selectable is TileObject tileObject) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(tileObject)) {
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