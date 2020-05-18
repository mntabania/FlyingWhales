namespace Quests.Steps {
    public class FlawTriggeredStep : QuestStep {
        public FlawTriggeredStep(string stepDescription) : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.FLAW_TRIGGERED_BY_PLAYER, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.FLAW_TRIGGERED_BY_PLAYER, Complete);
        }
    }
}