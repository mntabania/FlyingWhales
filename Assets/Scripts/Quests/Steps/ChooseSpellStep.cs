using System;
namespace Quests.Steps {
    public class ChooseSpellStep : QuestStep {
        private readonly Func<SpellData, bool> _validityChecker;
        public ChooseSpellStep(System.Func<SpellData, bool> validityChecker, string stepDescription) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SpellData>(Signals.PLAYER_SET_ACTIVE_SPELL, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SpellData>(Signals.PLAYER_SET_ACTIVE_SPELL, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SpellData chosenSpell) {
            if (_validityChecker.Invoke(chosenSpell)) {
                Complete();
            }
        }
        #endregion
    }
}