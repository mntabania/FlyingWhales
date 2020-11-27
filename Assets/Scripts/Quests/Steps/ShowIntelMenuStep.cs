namespace Quests.Steps {
    public class ShowIntelMenuStep : QuestStep {
        public ShowIntelMenuStep(string stepDescription = "Click Intel Tab") 
            : base(stepDescription) {
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener(UISignals.INTEL_MENU_OPENED, Complete);
            if (PlayerUI.Instance.intelContainer.activeSelf) {
                Complete();
            }
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(UISignals.INTEL_MENU_OPENED, Complete);
        }
    }
}