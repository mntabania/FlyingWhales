namespace Tutorial {
    public class ActivateHarassStep : TutorialQuestStep {
        public ActivateHarassStep(string stepDescription = "Click on Harass") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.HARASS_ACTIVATED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.HARASS_ACTIVATED, Complete);
        }
    }
}