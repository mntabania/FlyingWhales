namespace Quests.Steps {
    public class ShareIntelStep : QuestStep {
        public ShareIntelStep(string stepDescription = "Share Intel") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(UISignals.ON_SHARE_INTEL, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(UISignals.ON_SHARE_INTEL, Complete);
        }
    }
}