namespace Quests.Steps {
    public class SchemeUIShown : QuestStep {
        
        
        public SchemeUIShown(string stepDescription = "Click on any available scheme") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(UISignals.SCHEME_UI_SHOWN, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(UISignals.SCHEME_UI_SHOWN, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion() {
            Complete();
        }
        #endregion
    }
}