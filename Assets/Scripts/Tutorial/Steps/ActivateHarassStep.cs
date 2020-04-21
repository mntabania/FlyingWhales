namespace Tutorial {
    public class ActivateHarassStep : TutorialQuestStep {
        public ActivateHarassStep(string stepDescription = "Click on Harass", string tooltip = "") 
            : base(stepDescription, tooltip) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.HARASS_ACTIVATED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.HARASS_ACTIVATED, Complete);
        }
    }
}