namespace Quests.Steps {
    public class TriggerElectricChainStep : QuestStep {
        public TriggerElectricChainStep(string stepDescription = "Trigger Electric Chain") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.ELECTRIC_CHAIN_TRIGGERED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.ELECTRIC_CHAIN_TRIGGERED, Complete);
        }
    }
}