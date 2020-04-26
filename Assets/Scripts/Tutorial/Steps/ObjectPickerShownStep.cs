namespace Tutorial {
    public class ObjectPickerShownStep : TutorialQuestStep {
        private readonly string _neededIdentifier;
        public ObjectPickerShownStep(string stepDescription, string neededIdentifier) 
            : base(stepDescription) {
            _neededIdentifier = neededIdentifier;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<string>(Signals.OBJECT_PICKER_SHOWN, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<string>(Signals.OBJECT_PICKER_SHOWN, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(string identifier) {
            if (identifier == _neededIdentifier) {
                Complete();
            }
        }
        #endregion
    }
}