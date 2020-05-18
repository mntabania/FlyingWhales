namespace Quests.Steps {
    public class FlawClickedStep : QuestStep {
        public FlawClickedStep(string stepDescription) : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.FLAW_CLICKED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.FLAW_CLICKED, Complete);
        }
    }
}