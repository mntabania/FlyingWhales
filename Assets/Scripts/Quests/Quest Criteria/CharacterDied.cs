using System;
namespace Quests {
    public class CharacterDied : QuestCriteria {
        private readonly Func<Character, bool> _validityChecker;
        public Character character { get; private set; }
        public CharacterDied(Func<Character, bool> validityChecker) {
            _validityChecker = validityChecker;
        }
        
        public override void Enable() {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        public override void Disable() {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        
        private void OnCharacterDied(Character character) {
            if (_validityChecker != null) {
                if (_validityChecker.Invoke(character)) {
                    this.character = character;
                    SetCriteriaAsMet();
                }
            } else {
                this.character = character;
                SetCriteriaAsMet();
            }
        }
    }
}