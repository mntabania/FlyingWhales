namespace Quests.Steps {
    public class ExecuteAfflictionStep : QuestStep {
        public ExecuteAfflictionStep(string stepDescription) : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SpellData spellData) {
            Complete();
        }
        #endregion
    }
}