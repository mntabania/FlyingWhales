using Traits;
namespace Quests.Steps {
    public class FlawTriggeredStep : QuestStep {
        private readonly string _flawToTrigger;
        public FlawTriggeredStep(string stepDescription, string flawToTrigger) : base(stepDescription) {
            _flawToTrigger = flawToTrigger;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Trait>(Signals.FLAW_TRIGGERED_BY_PLAYER, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Trait>(Signals.FLAW_TRIGGERED_BY_PLAYER, CheckForCompletion);
        }

        private void CheckForCompletion(Trait trait) {
            if (trait.name == _flawToTrigger) {
                Complete();
            }
        }
    }
}