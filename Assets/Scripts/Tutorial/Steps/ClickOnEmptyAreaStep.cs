using System;
namespace Tutorial {
    public class ClickOnEmptyAreaStep : TutorialQuestStep {
        private readonly Func<HexTile, bool> _validityChecker;
        public ClickOnEmptyAreaStep(string stepDescription = "Click on an empty area", string tooltip = "", 
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
            //check first if selected hextile is indeed empty.
            if (selectable is HexTile hexTile && hexTile.settlementOnTile == null && hexTile.landmarkOnTile == null 
                && hexTile.elevationType != ELEVATION.WATER && hexTile.elevationType != ELEVATION.MOUNTAIN) {
                if (_validityChecker != null) { //if validity checker was provided, check that too.
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