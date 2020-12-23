namespace Quests.Steps {
    public class ParameterlessSignalReceived : QuestStep {

        private string _signalName;

        public ParameterlessSignalReceived(string p_signalName, string stepDescription) : base(stepDescription) {
            _signalName = p_signalName;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener(_signalName, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(_signalName, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion() {
            Complete();
        }
        #endregion
    }
}