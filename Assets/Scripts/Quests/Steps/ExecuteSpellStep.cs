using System;
namespace Quests.Steps {
    public class ExecuteSpellStep : QuestStep {
        private readonly Func<SkillData, bool> _validityChecker;
        private readonly PLAYER_SKILL_TYPE _neededSpellType;
        
        public ExecuteSpellStep(PLAYER_SKILL_TYPE neededSpellType, string stepDescription) 
            : base(stepDescription) {
            _neededSpellType = neededSpellType;
            _validityChecker = null;
        }
        public ExecuteSpellStep(System.Func<SkillData, bool> validityChecker, string stepDescription) 
            : base(stepDescription) {
            _validityChecker = validityChecker;
            _neededSpellType = PLAYER_SKILL_TYPE.NONE;
        }
        protected override void SubscribeListeners() {
            if (_neededSpellType == PLAYER_SKILL_TYPE.METEOR) {
                Messenger.AddListener(PlayerSkillSignals.METEOR_FELL, Complete);
            } else {
                Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, CheckForCompletion);    
            }
            
        }
        protected override void UnSubscribeListeners() {
            if (_neededSpellType == PLAYER_SKILL_TYPE.METEOR) {
                Messenger.RemoveListener(PlayerSkillSignals.METEOR_FELL, Complete);
            } else {
                Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, CheckForCompletion);
            }
        }

        #region Listeners
        private void CheckForCompletion(SkillData spellData) {
            if (_validityChecker != null) {
                if (_validityChecker.Invoke(spellData)) {
                    Complete();
                }
            } else if (spellData.type == _neededSpellType) {
                Complete();
            }
        }
        #endregion
    }
}