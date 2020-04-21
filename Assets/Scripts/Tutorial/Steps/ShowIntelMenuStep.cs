namespace Tutorial {
    public class ShowIntelMenuStep : TutorialQuestStep {
        public ShowIntelMenuStep(string stepDescription = "Click Intel Tab", string tooltip = "") 
            : base(stepDescription, tooltip) {
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.INTEL_MENU_OPENED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.INTEL_MENU_OPENED, Complete);
        }
    }
}