namespace Quests.Steps {
    public class ActivateDefendStep : QuestStep {
        public ActivateDefendStep(string stepDescription = "Click on Defend") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(PlayerSignals.DEFEND_ACTIVATED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(PlayerSignals.DEFEND_ACTIVATED, Complete);
        }
    }
}