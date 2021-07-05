using System.Linq;
namespace Quests {
    public class SpellExecuted : QuestCriteria {
        private readonly PLAYER_SKILL_TYPE[] _spellTypes;
        
        public SpellExecuted(PLAYER_SKILL_TYPE[] spellTypes) {
            _spellTypes = spellTypes;
        }
        
        public override void Enable() {
            Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnSpellExecuted);
            Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, OnSpellExecuted);
            Messenger.AddListener<PlayerAction>(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        public override void Disable() {
            Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnSpellExecuted);
            Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, OnSpellExecuted);
            Messenger.RemoveListener<PlayerAction>(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        
        private void OnSpellExecuted(SkillData spell) {
            if (_spellTypes.Contains(spell.type)) {
                SetCriteriaAsMet();
            } else {
                //other spell was cast
                SetCriteriaAsUnMet();
            }
        }
    }
}