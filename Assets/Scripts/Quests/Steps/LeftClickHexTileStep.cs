namespace Quests.Steps {
    public class LeftClickHexTileStep : QuestStep {
        public LeftClickHexTileStep(string stepDescription = "Left click on a hextile") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<HexTile>(AreaSignals.AREA_LEFT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<HexTile>(AreaSignals.AREA_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(HexTile tile) {
            Complete();
        }
        #endregion
    }
}