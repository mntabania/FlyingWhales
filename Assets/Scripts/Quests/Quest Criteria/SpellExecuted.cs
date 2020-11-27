﻿using System.Linq;
namespace Quests {
    public class SpellExecuted : QuestCriteria {
        private readonly SPELL_TYPE[] _spellTypes;
        
        public SpellExecuted(SPELL_TYPE[] spellTypes) {
            _spellTypes = spellTypes;
        }
        
        public override void Enable() {
            Messenger.AddListener<SpellData>(SpellSignals.ON_EXECUTE_SPELL, OnSpellExecuted);
            Messenger.AddListener<SpellData>(SpellSignals.ON_EXECUTE_AFFLICTION, OnSpellExecuted);
            Messenger.AddListener<PlayerAction>(SpellSignals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        public override void Disable() {
            Messenger.RemoveListener<SpellData>(SpellSignals.ON_EXECUTE_SPELL, OnSpellExecuted);
            Messenger.RemoveListener<SpellData>(SpellSignals.ON_EXECUTE_AFFLICTION, OnSpellExecuted);
            Messenger.RemoveListener<PlayerAction>(SpellSignals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        
        private void OnSpellExecuted(SpellData spell) {
            if (_spellTypes.Contains(spell.type)) {
                SetCriteriaAsMet();
            } else {
                //other spell was cast
                SetCriteriaAsUnMet();
            }
        }
    }
}