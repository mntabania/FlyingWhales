using System;
namespace Quests.Steps {
    public class ChooseSpellStep : QuestStep {
        private readonly Func<SkillData, bool> _validityChecker;
        public ChooseSpellStep(System.Func<SkillData, bool> validityChecker, string stepDescription) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SkillData chosenSpell) {
            if (_validityChecker.Invoke(chosenSpell)) {
                Complete();
            }
        }
        #endregion
    }
}