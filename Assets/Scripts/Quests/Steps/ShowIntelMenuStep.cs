namespace Quests.Steps {
    public class ShowIntelMenuStep : QuestStep {
        public ShowIntelMenuStep(string stepDescription = "Click Intel Tab") 
            : base(stepDescription) {
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.INTEL_MENU_OPENED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.INTEL_MENU_OPENED, Complete);
        }
    }
}