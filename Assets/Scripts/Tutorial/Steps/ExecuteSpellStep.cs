﻿namespace Tutorial {
    public class ExecuteSpellStep : TutorialQuestStep {
        private readonly SPELL_TYPE _neededSpellType;
        
        public ExecuteSpellStep(SPELL_TYPE neededSpellType, string stepDescription) 
            : base(stepDescription) {
            _neededSpellType = neededSpellType;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SpellData spellData) {
            if (spellData.type == _neededSpellType) {
                Complete();
            }
        }
        #endregion
    }
}