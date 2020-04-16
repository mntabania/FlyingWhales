namespace Tutorial {
    public class ClickOnEmptyAreaStep : TutorialQuestStep {
        public ClickOnEmptyAreaStep(string stepDescription = "Click on an empty area", string tooltip = "") 
            : base(stepDescription, tooltip) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(ISelectable selectable) {
            if (selectable is HexTile hexTile && hexTile.settlementOnTile == null && hexTile.landmarkOnTile == null 
                && hexTile.elevationType != ELEVATION.WATER && hexTile.elevationType != ELEVATION.MOUNTAIN) {
                Complete();
            }
        }
        #endregion
    }
}