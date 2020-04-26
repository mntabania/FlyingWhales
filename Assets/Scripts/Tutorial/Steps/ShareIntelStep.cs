namespace Tutorial {
    public class ShareIntelStep : TutorialQuestStep {
        public ShareIntelStep(string stepDescription = "Share Intel") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.ON_OPEN_SHARE_INTEL, Complete);
        }
    }
}