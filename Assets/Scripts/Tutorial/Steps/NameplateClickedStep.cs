using System;
namespace Tutorial {
    public class NameplateClickedStep : TutorialQuestStep {
        private string _neededText;

        public NameplateClickedStep(string neededText, string stepDescription = "Nameplate clicked", string tooltip = "")
            : base(stepDescription) {
            _neededText = neededText;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<string>(Signals.NAMEPLATE_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<string>(Signals.NAMEPLATE_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(string text) {
            if (text == _neededText) {
                Complete();
            }
        }
        #endregion
    }
}