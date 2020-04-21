namespace Tutorial {
    public class DoubleClickHexTileStep : TutorialQuestStep {
        public DoubleClickHexTileStep(string stepDescription = "Double click on a hextile", string tooltip = "") : base(stepDescription, tooltip) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(HexTile tile) {
            Complete();
        }
        #endregion
    }
}