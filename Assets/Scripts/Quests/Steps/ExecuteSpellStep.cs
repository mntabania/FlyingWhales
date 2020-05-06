namespace Quests.Steps {
    public class ExecuteSpellStep : QuestStep {
        private readonly SPELL_TYPE _neededSpellType;
        
        public ExecuteSpellStep(SPELL_TYPE neededSpellType, string stepDescription) 
            : base(stepDescription) {
            _neededSpellType = neededSpellType;
        }
        protected override void SubscribeListeners() {
            if (_neededSpellType == SPELL_TYPE.METEOR) {
                Messenger.AddListener(Signals.METEOR_FELL, Complete);
            } else {
                Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, CheckForCompletion);    
            }
            
        }
        protected override void UnSubscribeListeners() {
            if (_neededSpellType == SPELL_TYPE.METEOR) {
                Messenger.RemoveListener(Signals.METEOR_FELL, Complete);
            } else {
                Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, CheckForCompletion);
            }
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