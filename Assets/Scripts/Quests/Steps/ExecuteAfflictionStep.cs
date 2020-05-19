namespace Quests.Steps {
    public class ExecuteAfflictionStep : QuestStep {
        private readonly SPELL_TYPE _requiredAffliction;
        public ExecuteAfflictionStep(string stepDescription, SPELL_TYPE requiredAffliction = SPELL_TYPE.NONE) : base(stepDescription) {
            _requiredAffliction = requiredAffliction;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SpellData spellData) {
            if (_requiredAffliction == SPELL_TYPE.NONE || _requiredAffliction == spellData.type) {
                Complete();    
            }
        }
        #endregion
    }
}