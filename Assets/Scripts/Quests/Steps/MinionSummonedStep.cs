using System;
namespace Quests.Steps {
    public class MinionSummonedStep : QuestStep {
        private readonly Func<Minion, bool> _validityChecker;
        public MinionSummonedStep(System.Func<Minion, bool> validityChecker, string stepDescription) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Minion>(PlayerSkillSignals.SUMMON_MINION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Minion>(PlayerSkillSignals.SUMMON_MINION, CheckForCompletion);
        }

        private void CheckForCompletion(Minion minion) {
            if (_validityChecker.Invoke(minion)) {
                Complete();
            }
        }
    }
}