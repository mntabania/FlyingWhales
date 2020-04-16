using System;
namespace Tutorial {
    public class ChooseSpellStep : TutorialQuestStep {
        private readonly SPELL_TYPE _neededSpellType;
        public ChooseSpellStep(SPELL_TYPE neededSpellType, string stepDescription, string tooltip = "") 
            : base(stepDescription, tooltip) {
            _neededSpellType = neededSpellType;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SpellData>(Signals.PLAYER_SET_ACTIVE_SPELL, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SpellData>(Signals.PLAYER_SET_ACTIVE_SPELL, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SpellData chosenSpell) {
            if (chosenSpell.type == _neededSpellType) {
                Complete();
            }
        }
        #endregion
    }
}