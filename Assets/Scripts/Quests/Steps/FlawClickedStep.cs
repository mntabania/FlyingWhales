using Traits;
namespace Quests.Steps {
    public class FlawClickedStep : QuestStep {
        private readonly string _requiredTraitName;
        public FlawClickedStep(string stepDescription, string requiredTraitName) : base(stepDescription) {
            _requiredTraitName = requiredTraitName;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Trait>(Signals.FLAW_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Trait>(Signals.FLAW_CLICKED, CheckForCompletion);
        }

        #region Completion
        private void CheckForCompletion(Trait trait) {
            if (_requiredTraitName == trait.name) {
                Complete();
            }
        }
        #endregion
    }
}