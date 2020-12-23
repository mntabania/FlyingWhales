namespace Quests.Steps {
    public class TemptationPopupShown : QuestStep {
        
        
        public TemptationPopupShown(string stepDescription = "Click on the Tempt button") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(UISignals.TEMPTATIONS_POPUP_SHOWN, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(UISignals.TEMPTATIONS_POPUP_SHOWN, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion() {
            Complete();
        }
        #endregion
    }
}