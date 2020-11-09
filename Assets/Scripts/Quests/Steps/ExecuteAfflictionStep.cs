using System;
namespace Quests.Steps {
    public class ExecuteAfflictionStep : QuestStep {
        private readonly SPELL_TYPE _requiredAffliction;
        private readonly Action<Character> _onAfflictCallback;
        
        public ExecuteAfflictionStep(string stepDescription, SPELL_TYPE requiredAffliction = SPELL_TYPE.NONE, 
            System.Action<Character> onAfflictCallback = null) : base(stepDescription) {
            _requiredAffliction = requiredAffliction;
            _onAfflictCallback = onAfflictCallback;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SpellData>(SpellSignals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SpellData>(SpellSignals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SpellData spellData) {
            if (_requiredAffliction == SPELL_TYPE.NONE || _requiredAffliction == spellData.type) {
                Complete();
                if (_onAfflictCallback != null) {
                    Character currentlySelectedCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
                    if (currentlySelectedCharacter != null) {
                        _onAfflictCallback.Invoke(currentlySelectedCharacter);    
                    }
                }
            }
        }
        #endregion
    }
}