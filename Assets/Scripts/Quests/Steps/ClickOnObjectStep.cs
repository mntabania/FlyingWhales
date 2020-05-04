namespace Quests.Steps {
    public class ClickOnObjectStep : QuestStep {
        public ClickOnObjectStep(string stepDescription = "Click on an object") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(ISelectable selectable) {
            if (selectable is TileObject) {
                Complete();
            }
        }
        #endregion
    }
}