namespace Quests.Steps {
    public class ActivateHarassStep : QuestStep {
        public ActivateHarassStep(string stepDescription = "Click on Harass") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(PlayerSignals.HARASS_ACTIVATED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(PlayerSignals.HARASS_ACTIVATED, Complete);
        }
    }
}