namespace Quests.Steps {
    public class TriggerElectricChainStep : QuestStep {
        public TriggerElectricChainStep(string stepDescription = "Trigger Electric Chain") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(PlayerSignals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(PlayerSignals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER, Complete);
        }
    }
}