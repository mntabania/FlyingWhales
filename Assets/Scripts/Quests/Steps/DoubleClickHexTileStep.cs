namespace Quests.Steps {
    public class DoubleClickHexTileStep : QuestStep {
        public DoubleClickHexTileStep(string stepDescription = "Double click on a hextile") : base(stepDescription) { }
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