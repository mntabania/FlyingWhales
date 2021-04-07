using System;
namespace Quests.Steps {
    public class ExecuteAfflictionStep : QuestStep {
        private readonly PLAYER_SKILL_TYPE _requiredAffliction;
        private readonly Action<Character> _onAfflictCallback;
        
        public ExecuteAfflictionStep(string stepDescription, PLAYER_SKILL_TYPE requiredAffliction = PLAYER_SKILL_TYPE.NONE, 
            System.Action<Character> onAfflictCallback = null) : base(stepDescription) {
            _requiredAffliction = requiredAffliction;
            _onAfflictCallback = onAfflictCallback;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(SkillData spellData) {
            if (_requiredAffliction == PLAYER_SKILL_TYPE.NONE || _requiredAffliction == spellData.type) {
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